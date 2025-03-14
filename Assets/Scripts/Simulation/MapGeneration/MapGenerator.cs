using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    private const int MaxOutlineSteps = 10000;
    // In clockwise order, starting with because that direction should be checked first when iterating over the texture.
    public static readonly Vector2Int[] CardinalDirections = {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};

    [SerializeField] private Texture2D mapImage;
    [SerializeField] private Province provincePrefab;
    [SerializeField] private Transform background;
    [SerializeField] private Transform provinceParent;
    
    private readonly Dictionary<Color, ProvinceGenerator> provinceGenerators = new();
    private readonly Dictionary<Color, Province> provinces = new();
    private int width, height;
    private Vector2 worldSpaceOffset;
    private float worldSpaceScale;
    
    private void Awake(){
        width = mapImage.width;
        height = mapImage.height;
        
        Vector2 scale2D = background.transform.lossyScale;
        worldSpaceScale = scale2D.x/width;
        worldSpaceOffset = -scale2D/2;
        
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                Color color = mapImage.GetPixel(x, y);
                if (!provinceGenerators.ContainsKey(color)){
                    FindProvinceOutline(color, new Vector2Int(x, y));
                }
            }
        }
        
        foreach ((Color color, ProvinceGenerator provinceGenerator) in provinceGenerators){
            provinceGenerator.GenerateData();
            Vector3 position = ConvertToWorldSpace(provinceGenerator.Center)+Vector3.up;
            Province province = Instantiate(provincePrefab, position, Quaternion.identity, provinceParent);
            province.Init(color, provinceGenerator.OutlineMesh, provinceGenerator.ShapeMesh);
            provinces.Add(color, province);
            // instantiate province prefab
            // call province class init and assign data from generator
        }
        
        // create province graph
        foreach ((Color color, Province province) in provinces){
            // add province to graph
            // convert connections from color values to graph links
        }
        
        // Destroy this component after generation is done. Don't destroy the gameObject.
        // Leave commented out until visual debugging becomes possible in the Province components instead.
        //Destroy(this);
    }
    
    private void FindProvinceOutline(Color color, Vector2Int startPosition){
        ProvinceGenerator province = new();
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
                if (newPosition.x < 0 || width <= newPosition.x || newPosition.y < 0 || height <= newPosition.y){
                    continue;
                }
                Color newPixelColor = mapImage.GetPixel(newPosition.x, newPosition.y);
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
    
    private Vector3 ConvertToWorldSpace(Vector2 vector){
        return VectorGeometry.ToXZPlane(vector*worldSpaceScale + worldSpaceOffset);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos(){
        foreach ((Color color, ProvinceGenerator provinceGenerator) in provinceGenerators){
            Handles.color = color;
            provinceGenerator.GizmosPolygon(ConvertToWorldSpace);
        }
    }
#endif
}