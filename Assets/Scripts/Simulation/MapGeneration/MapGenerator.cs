using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    private const int MaxOutlineSteps = 10000;
    // In clockwise order, starting with because that direction should be checked first when iterating over the texture.
    private static readonly Vector2Int[] CardinalDirections = {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};

    [SerializeField] private Texture2D mapImage;

    private readonly Dictionary<Color, (List<Vector2Int>, HashSet<Color>)> provinceOutlines = new();
    private readonly Dictionary<Color, Province> provinces = new();
    private int width, height;
    private void Awake(){
        width = mapImage.width;
        height = mapImage.height;
        for (int y = 0; y < height; y++){
            for (int x = 0; x < width; x++){
                Color color = mapImage.GetPixel(x, y);
                if (!provinceOutlines.ContainsKey(color)){
                    FindProvinceOutline(new Vector2Int(x, y), color);
                }
            }
        }
        foreach ((Color color, (List<Vector2Int> outlinePixels, HashSet<Color> neighbors)) in provinceOutlines){
            print("\n\n"+color);
            StringBuilder outlineStringBuilder = new("Coordinates: ");
            foreach (Vector2Int outlinePixel in outlinePixels){
                outlineStringBuilder.Append(outlinePixel);
                outlineStringBuilder.Append(", ");
            }
            print(outlineStringBuilder);
            /*StringBuilder neighborStringBuilder = new("Neighbors: ");
            foreach (Color neighbor in neighbors){
                neighborStringBuilder.Append(neighbor);
                neighborStringBuilder.Append(", ");
            }
            print(neighborStringBuilder);*/
            // calculate center
            // instantiate province prefab
            // call province class init:
            // generate vertex list
            // generate shape mesh
            // generate outline mesh
        }
        // create province graph
        foreach ((Color color, Province province) in provinces){
            // add province to graph
            // convert connections from color values to graph links
        }
        
        // Destroy self
    }
    private void FindProvinceOutline(Vector2Int startPosition, Color color){
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
        
        provinceOutlines.Add(color, (outlinePixels, neighbors));   
    }
}
