using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mathematics;
using UnityEngine;

[RequireComponent(typeof(MapGraph))]
public class MapGenerator : MonoBehaviour {
    private const int MaxOutlineSteps = 10000;
    // In clockwise order, starting with because that direction should be checked first when iterating over the texture.
    public static readonly Vector2Int[] CardinalDirections = {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};

    [SerializeField] private Texture2D mapImage;
    [SerializeField] private Province provincePrefab;
    [SerializeField] private Transform provinceParent;
    [SerializeField] private float borderWidth;
    [SerializeField] private float mapWidth;
    
    private Color32[] mapPixels;
    private int imageWidth, imageHeight;
    private Vector2 worldSpaceOffset;
    private float worldSpaceScale;
    
    private void Awake(){
        imageWidth = mapImage.width;
        imageHeight = mapImage.height;
        
        worldSpaceScale = mapWidth/imageWidth;
        worldSpaceOffset = -0.5f*new Vector2(mapWidth, mapWidth*imageHeight/imageWidth);
        
        mapPixels = mapImage.GetPixels32();
        Dictionary<Color32, Vector2Int> provincePositions = new();
        for (int y = 0; y < imageHeight; y++){
            for (int x = 0; x < imageWidth; x++){
                Color32 color = GetPixel(x, y);
                if (!provincePositions.ContainsKey(color)){
                    provincePositions.Add(color, new Vector2Int(x, y));
                }
            }
        }
        
        // Multithreading reduced time from 18ms to 11ms when tested on March 20th's provinces.png file.
        Thread[] threads = new Thread[Environment.ProcessorCount];
        KeyValuePair<Color32, Vector2Int>[] provincePositionArray = provincePositions.ToArray();
        (Color32, ProvinceGenerator)[] provinceGenerators = new(Color32, ProvinceGenerator)[provincePositionArray.Length];
        for (int i = 0; i < threads.Length; i++){
            int startIndex = i;
            threads[i] = new Thread(() => {
                for (int j = startIndex; j < provincePositionArray.Length; j += threads.Length){
                    Color32 color = provincePositionArray[j].Key;
                    ProvinceGenerator provinceGenerator = FindProvinceOutline(color, provincePositionArray[j].Value);
                    provinceGenerator.GenerateData();
                    provinceGenerators[j] = (color, provinceGenerator);
                }
            });
            threads[i].Start();
        }
        foreach (Thread thread in threads){
            thread.Join();
        }
        
        Vector3 provinceScale = new(worldSpaceScale, 1, worldSpaceScale);
        HashSet<(Province province, HashSet<Color32>)> provinceNeighbors = new();
        MapGraph mapGraph = GetComponent<MapGraph>();
        foreach ((Color32 color, ProvinceGenerator provinceGenerator) in provinceGenerators){
            Vector3 worldPosition = ConvertToWorldSpace(provinceGenerator.Pivot);
            Province province = Instantiate(provincePrefab, worldPosition, Quaternion.identity, provinceParent);
            province.transform.localScale = provinceScale;
            province.Init(color, provinceGenerator.Pivot, provinceGenerator.OutlineMesh, provinceGenerator.ShapeMesh);
            provinceNeighbors.Add((province, provinceGenerator.Neighbors));
            mapGraph.Add(province);
        }
        
        foreach ((Province province, HashSet<Color32> neighbors) in provinceNeighbors){
            foreach (Color32 neighborColor in neighbors){
                province.AddNeighbor(mapGraph[neighborColor]);
            }
        }
        
        // Destroy this component after generation is done since it's purpose has been achieved. Don't destroy the gameObject.
        Destroy(this);
    }
    
    private ProvinceGenerator FindProvinceOutline(Color32 color, Vector2Int startPosition){
        ProvinceGenerator province = new(borderWidth);
        List<Vector2Int> outlinePixels = new();
        HashSet<Color32> neighbors = new();
        outlinePixels.Add(startPosition);
        Vector2Int position = startPosition;
        // You're moving right during the full mapImage iteration, so the up direction (with index 0) is a turn to the left.
        int leftTurnIndex = 0;
        int steps = 0;
        do {
            for (int i = 0; i < CardinalDirections.Length; i++){
                int directionIndex = (leftTurnIndex+i+CardinalDirections.Length) % CardinalDirections.Length;
                Vector2Int newPosition = position+CardinalDirections[directionIndex];
                if (newPosition.x < 0 || imageWidth <= newPosition.x || newPosition.y < 0 || imageHeight <= newPosition.y){
                    continue;
                }
                Color32 newPixelColor = GetPixel(newPosition.x, newPosition.y);
                if (!color.Equals(newPixelColor)){
                    neighbors.Add(newPixelColor);
                    continue;
                }
                if (i != 0){
                    outlinePixels.Add(newPosition);
                } else {
                    // If a left turn was made, override the last pixel in the list because it's an inside corner.
                    outlinePixels[^1] = newPosition;
                }
                position = newPosition;
                // Subtracting one gives a left turn since the directions are in clockwise order.
                leftTurnIndex = directionIndex-1;
                break;
            }
            steps++;
        } while (outlinePixels[^1] != startPosition && steps < MaxOutlineSteps);

        // Remove the last pixel if it's a duplicate of the first.
        if (outlinePixels[^1] == outlinePixels[0]){
            outlinePixels.RemoveAt(outlinePixels.Count-1);
        }
        
        province.OutlinePixels.AddRange(outlinePixels);
        province.Neighbors.UnionWith(neighbors);
        return province;
    }

    private Color32 GetPixel(int x, int y){
        return mapPixels[x+y*imageWidth];
    }
    
    private Vector3 ConvertToWorldSpace(Vector2 vector){
        return VectorGeometry.ToXZPlane(vector*worldSpaceScale + worldSpaceOffset);
    }
/*#if UNITY_EDITOR
    private void OnDrawGizmos(){
        foreach ((Color color, ProvinceGenerator provinceGenerator) in provinceGenerators){
            Handles.color = color;
            provinceGenerator.GizmosPolygon(ConvertToWorldSpace);
        }
    }
#endif*/
}