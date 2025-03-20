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
    private MapGraph mapGraph;
    
    private readonly Dictionary<Color, ProvinceGenerator> provinceGenerators = new();
    private readonly Dictionary<Color, Province> provinces = new();
    private Color[] mapPixels;
    private int imageWidth, imageHeight;
    private Vector2 worldSpaceOffset;
    private float worldSpaceScale;
    
    private void Awake(){
        imageWidth = mapImage.width;
        imageHeight = mapImage.height;
        
        worldSpaceScale = mapWidth/imageWidth;
        worldSpaceOffset = -0.5f*new Vector2(mapWidth, mapWidth*imageHeight/imageWidth);
        
        mapPixels = mapImage.GetPixels();
        for (int y = 0; y < imageHeight; y++){
            for (int x = 0; x < imageWidth; x++){
                Color color = GetPixel(x, y);
                if (!provinceGenerators.ContainsKey(color)){
                    FindProvinceOutline(color, new Vector2Int(x, y));
                }
            }
        }
        
        int threadCount = Environment.ProcessorCount;
        ProvinceGenerator[] provinceGeneratorArray = provinceGenerators.Values.ToArray();
        Thread[] threads = new Thread[threadCount];
        for (int i = 0; i < threads.Length; i++){
            int startIndex = i;
            threads[startIndex] = new Thread(() => {
                for (int j = startIndex; j < provinceGeneratorArray.Length; j += threadCount){
                    provinceGeneratorArray[j].GenerateData();
                }
            });
            threads[i].Start();
        }
        foreach (Thread thread in threads){
            thread.Join();
        }
        
        foreach ((Color color, ProvinceGenerator provinceGenerator) in provinceGenerators){
            Vector3 position = ConvertToWorldSpace(provinceGenerator.Pivot);
            Province province = Instantiate(provincePrefab, position, Quaternion.identity, provinceParent);
            province.transform.localScale = new Vector3(worldSpaceScale, 1, worldSpaceScale);
            province.Init(color, provinceGenerator.Pivot, provinceGenerator.OutlineMesh, provinceGenerator.ShapeMesh);
            provinces.Add(color, province);
        }
        
        mapGraph = GetComponent<MapGraph>();
        foreach ((Color color, Province province) in provinces){
            mapGraph.Add(color, province);
            foreach (Color neighborColor in provinceGenerators[color].Neighbors){
                province.AddNeighbor(provinces[neighborColor]);
            }
        }
        
        // Destroy this component after generation is done. Don't destroy the gameObject.
        Destroy(this);
    }
    
    private void FindProvinceOutline(Color color, Vector2Int startPosition){
        ProvinceGenerator province = new(borderWidth);
        List<Vector2Int> outlinePixels = new();
        HashSet<Color> neighbors = new();
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
                Color newPixelColor = GetPixel(newPosition.x, newPosition.y);
                if (newPixelColor != color){
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
        provinceGenerators.Add(color, province);
    }

    private Color GetPixel(int x, int y){
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