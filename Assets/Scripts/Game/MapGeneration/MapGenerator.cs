using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mathematics;
using Simulation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapGeneration {
    [RequireComponent(typeof(MapGraph))]
    public class MapGenerator : MonoBehaviour {
        private const int MaxOutlineSteps = 10000;
        // In clockwise order, starting with because that direction should be checked first when iterating over the texture.
        public static readonly Vector2Int[] CardinalDirections = {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};
        public static Color32 OffMapColorKey = new(0, 0, 0, 0);
        
        [SerializeField] private Texture2D mapImage;
        [SerializeField] private Land landPrefab;
        [SerializeField] private Sea seaPrefab;
        [SerializeField] private Country countryPrefab;
        [SerializeField] private Provinces[] provinceData;
        [SerializeField] private Countries countryData;
        [SerializeField] private bool doRandomizeCountries;
        [SerializeField] private Transform provinceParent;
        [SerializeField] private Transform countryParent;
        [SerializeField] private float borderWidth;
        [SerializeField] private float textureScale;
        [SerializeField] private float mapWidth;
        
        private MapGraph mapGraph;
        
        private Color32[] mapPixels;
        private readonly Dictionary<Color32, Vector2Int> provincePositions = new();
        private readonly Dictionary<Color32, ProvinceGenerator> provinceGenerators = new();
        private readonly Dictionary<Color32, ProvinceData> provinceDataDictionary = new();
        private readonly HashSet<(Province province, (List<Color32>, List<int>))> provinceNeighbors = new();
        
        private int imageWidth, imageHeight;
        private Vector2 worldSpaceOffset;
        private float worldSpaceScale;
        private Vector3 provinceScale;
        
        private void Awake(){
            mapGraph = GetComponent<MapGraph>();
            mapPixels = mapImage.GetPixels32();
            
            imageWidth = mapImage.width;
            imageHeight = mapImage.height;
            
            worldSpaceScale = mapWidth/imageWidth;
            worldSpaceOffset = -0.5f*new Vector2(mapWidth, mapWidth*imageHeight/imageWidth);
            provinceScale = new Vector3(worldSpaceScale, 1, worldSpaceScale);
            
            FindProvincePositions();
            GenerateProvinceVertices();
            SyncOutlineSegmentVertices();
            PutSerializedProvinceDataInDictionary();
            SpawnProvinceGameObjects();
            LinkNeighboringProvinces();
            InitializeCountries();
            
            // Destroy this component after generation is done since it's purpose has been achieved. Don't destroy the gameObject.
            Destroy(this);
        }
        
        private void FindProvincePositions(){
            for (int y = 0; y < imageHeight; y++){
                for (int x = 0; x < imageWidth; x++){
                    Color32 color = GetPixel(x, y);
                    if (!provincePositions.ContainsKey(color)){
                        provincePositions.Add(color, new Vector2Int(x, y));
                    }
                }
            }
        }
        
        private void GenerateProvinceVertices(){
            foreach ((Color32 color, Vector2Int position) in provincePositions){
                ProvinceGenerator provinceGenerator = FindProvinceOutline(color, position);
                provinceGenerator.CreateVertices();
                provinceGenerators.Add(color, provinceGenerator);
            }
        }
        private ProvinceGenerator FindProvinceOutline(Color32 color, Vector2Int startPosition){
            ProvinceGenerator province = new(borderWidth, textureScale);
            List<Vector2Int> outlinePixels = new();
            List<int> triPointIndices = new();
            List<Color32> neighbors = new();
            outlinePixels.Add(startPosition);
            Vector2Int position = startPosition-CardinalDirections[0];
            // You're moving right during the full mapImage iteration, so the up direction (with index 0) is a turn to the left.
            int leftTurnIndex = 0;
            Color32? previousNeighborColor = null;
            Color32? firstNeighborColor = null;
            int steps = 0;
            do {
                for (int i = 0; i < CardinalDirections.Length; i++){
                    int directionIndex = (leftTurnIndex+i+CardinalDirections.Length) % CardinalDirections.Length;
                    Vector2Int newPosition = position+CardinalDirections[directionIndex];
                    Color32 newPixelColor;
                    if (0 <= newPosition.x && newPosition.x < imageWidth && 0 <= newPosition.y && newPosition.y < imageHeight){
                        newPixelColor = GetPixel(newPosition.x, newPosition.y);
                    } else {
                        newPixelColor = OffMapColorKey;
                    }
                    if (!color.Equals(newPixelColor)){
                        firstNeighborColor ??= newPixelColor;
                        if (neighbors.Count == 0 || !neighbors[0].Equals(newPixelColor) && !neighbors[^1].Equals(newPixelColor)){
                            neighbors.Add(newPixelColor);
                        }
                        if (previousNeighborColor != null && !previousNeighborColor.Equals(newPixelColor)){
                            triPointIndices.Add(outlinePixels.Count-1);
                        }
                        previousNeighborColor = newPixelColor;
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
            } while ((outlinePixels[^1] != startPosition || outlinePixels.Count <= 1) && steps < MaxOutlineSteps);
            
            // Remove the last pixel if it's a duplicate of the first.
            if (outlinePixels[^1] == outlinePixels[0]){
                outlinePixels.RemoveAt(outlinePixels.Count-1);
            }
            // Checking for a tri-point exactly where the loop connects.
            if (!firstNeighborColor.Equals(previousNeighborColor)){
                triPointIndices.Insert(0, 0);
                neighbors.Insert(0, neighbors[^1]);
                neighbors.RemoveAt(neighbors.Count-1);
            }
            
            province.OutlinePixels.AddRange(outlinePixels);
            province.Neighbors.AddRange(neighbors);
            province.TriPointIndices.AddRange(triPointIndices);
            return province;
        }
        
        private void SyncOutlineSegmentVertices(){
            foreach ((Color32 color, ProvinceGenerator provinceGenerator) in provinceGenerators){
                for (int segmentIndex = 0; segmentIndex < provinceGenerator.Neighbors.Count; segmentIndex++){
                    Color32 neighborColor = provinceGenerator.Neighbors[segmentIndex];
                    provinceGenerators.TryGetValue(neighborColor, out ProvinceGenerator neighbor);
                    if (neighbor == null){
                        continue;
                    }
                    (int startIndex, int endIndex, int length) oldSegment = GetSegment(provinceGenerator, segmentIndex);
                    int neighborSegmentIndex = neighbor.Neighbors.FindIndex(color32 => color32.Equals(color));
                    (int startIndex, int endIndex, int length) newSegment = GetSegment(neighbor, neighborSegmentIndex);
                    int longerLength = Mathf.Max(oldSegment.length, newSegment.length);
                    for (int i = 0; i < longerLength; i++){
                        int vertexIndex = Mod(oldSegment.startIndex+i, provinceGenerator.Vertices);
                        int neighborVertexIndex = Mod(newSegment.endIndex-i, neighbor.Vertices);
                        if (i < oldSegment.length && i < newSegment.length){
                            provinceGenerator.Vertices[vertexIndex] = neighbor.Vertices[neighborVertexIndex];
                        } else if (i < newSegment.length){
                            provinceGenerator.Vertices.Insert(vertexIndex, neighbor.Vertices[neighborVertexIndex]);
                        } else {
                            provinceGenerator.Vertices.RemoveAt(vertexIndex);
                        }
                    }
                    if (oldSegment.length != newSegment.length){
                        int change = newSegment.length-oldSegment.length;
                        for (int i = segmentIndex; i < provinceGenerator.TriPointIndices.Count; i++){
                            provinceGenerator.TriPointIndices[i] += change;
                            while (provinceGenerator.TriPointIndices[i] < 0){
                                provinceGenerator.Vertices.Insert(0, provinceGenerator.Vertices[^1]);
                                provinceGenerator.Vertices.RemoveAt(provinceGenerator.Vertices.Count-1);
                                for (int j = 0; j < provinceGenerator.TriPointIndices.Count; j++){
                                    provinceGenerator.TriPointIndices[j]++;
                                }
                            }
                        }
                    }
                    FixCorner(segmentIndex-1, -1, provinceGenerator, color, neighbor.Vertices[newSegment.endIndex]);
                    FixCorner(segmentIndex+1, +0, provinceGenerator, color, neighbor.Vertices[newSegment.startIndex]);
                }
                provinceGenerator.RemoveDuplicateVertices();
            }
        }
        private void FixCorner(int neighborIndex, int indexOffset, ProvinceGenerator provinceGenerator, Color32 color, Vector2 vertex){
            neighborIndex = Mod(neighborIndex, provinceGenerator.Neighbors);
            Color32 neighborColor = provinceGenerator.Neighbors[neighborIndex];
            provinceGenerators.TryGetValue(neighborColor, out ProvinceGenerator neighbor);
            if (neighbor == null){
                return;
            }
            int neighborSegmentIndex = Mod(neighbor.Neighbors.FindIndex(color32 => color32.Equals(color))+indexOffset, neighbor.Neighbors);
            int index = neighbor.TriPointIndices[neighborSegmentIndex];
            neighbor.Vertices[index] = vertex;
        }
        private static (int, int, int) GetSegment(ProvinceGenerator provinceGenerator, int segmentIndex){
            int startIndex = provinceGenerator.TriPointIndices[Mod(segmentIndex-1, provinceGenerator.Neighbors)];
            int endIndex = provinceGenerator.TriPointIndices[segmentIndex];
            int segmentLength = Mod(endIndex-startIndex, provinceGenerator.Vertices)+1;
            return (startIndex, endIndex, segmentLength);
        }
        private static int Mod(int n, IList list){
            return Mod(n, list.Count);
        }
        private static int Mod(int n, int modulus){
            return (n+modulus)%modulus;
        }
        
        private void PutSerializedProvinceDataInDictionary(){
            foreach (Provinces provinces in provinceData){
                foreach (ProvinceData province in provinces.List){
                    provinceDataDictionary.Add(province.Color, province);
                }
            }
        }
        
        private void SpawnProvinceGameObjects(){
            foreach ((Color32 color, ProvinceGenerator provinceGenerator) in provinceGenerators){
                provinceGenerator.GenerateData();
                Vector3 worldPosition = ConvertToWorldSpace(provinceGenerator.Pivot);
                Province province;
                if (provinceDataDictionary.TryGetValue(color, out ProvinceData data)){
                    Land land = Instantiate(landPrefab, worldPosition, Quaternion.identity, provinceParent);
                    land.Init(color, mapGraph, data, provinceGenerator.Pivot, provinceGenerator.OutlineMesh, provinceGenerator.ShapeMesh, provinceGenerator.Vertices);
                    province = land.Province;
                } else {
                    Sea sea = Instantiate(seaPrefab, worldPosition, Quaternion.identity, provinceParent);
                    sea.Init(color, mapGraph, provinceGenerator.Pivot, provinceGenerator.OutlineMesh, provinceGenerator.ShapeMesh, provinceGenerator.Vertices);
                    province = sea.Province;
                }
                province.transform.localScale = provinceScale;
                provinceNeighbors.Add((province, (provinceGenerator.Neighbors, provinceGenerator.TriPointIndices)));
            }
        }

        private void LinkNeighboringProvinces(){
            foreach ((Province province, (List<Color32> neighborColors, List<int> triPointIndices)) in provinceNeighbors){
                for (int i = 0; i < neighborColors.Count; i++){
                    province.AddNeighbor(mapGraph[neighborColors[i]], triPointIndices[i], ConvertToWorldSpace);
                }
                province.CompleteSegmentLoop(ConvertToWorldSpace);
            }
        }
        
        private Color32 GetPixel(int x, int y){
            return mapPixels[x+y*imageWidth];
        }
        
        private Vector3 ConvertToWorldSpace(Vector2 vector){
            return VectorGeometry.ToXZPlane(vector*worldSpaceScale + worldSpaceOffset);
        }

        private void InitializeCountries(){
            Vector3 worldPosition = ConvertToWorldSpace(Vector2.zero);
            foreach (CountryData data in countryData.List){
                Country country = Instantiate(countryPrefab, worldPosition, Quaternion.identity, countryParent);
                country.Init(data, mapGraph);
                country.transform.localScale = provinceScale;
            }
    #if UNITY_EDITOR
            if (doRandomizeCountries){
                RandomizeCountries();
            }
    #endif
        }
    #if UNITY_EDITOR
        
        private void RandomizeCountries(){
            Country[] countries = countryParent.GetComponentsInChildren<Country>();
            int maxProvinces = -1;
            foreach (Country country in countries){
                int provinceCount = country.Provinces.Count();
                if (maxProvinces < provinceCount){
                    maxProvinces = provinceCount;
                }
            }
            int landProvincesLeft = provinceDataDictionary.Count;
            HashSet<Land> unownedLandProvinces = new();
            foreach (ProvinceData data in provinceDataDictionary.Values){
                Land landProvince = mapGraph[data.Color].Land;
                landProvince.Owner = null;
                unownedLandProvinces.Add(landProvince);
            }
            Queue<Land> provinceCrawl = new();
            provinceCrawl.Enqueue(RandomUnownedProvince());
            for (int i = countries.Length-1; i >= 0; i--){
                int countryProvinceCount = Mathf.Max(Mathf.Min(Random.Range(1, maxProvinces+1), landProvincesLeft-i), (landProvincesLeft-i)/(i+1));
                for (int j = 0; j < countryProvinceCount; j++){
                    Land land = provinceCrawl.Dequeue();
                    land.Owner = countries[i];
                    unownedLandProvinces.Remove(land);
                    landProvincesLeft--;
                    if (provinceCrawl.Count != 0 || landProvincesLeft == 0){
                        continue;
                    }
                    List<Land> unownedNeighbors = new();
                    foreach (ProvinceLink link in land.Province.Links){
                        Province neighbor = link.Target;
                        if (neighbor.IsLand && !neighbor.Land.HasOwner){
                            unownedNeighbors.Add(neighbor.Land);
                        }
                    }
                    for (int k = unownedNeighbors.Count-1; k > 0; k--){
                        int randomIndex = Random.Range(0, k+1);
                        (unownedNeighbors[k], unownedNeighbors[randomIndex]) = (unownedNeighbors[randomIndex], unownedNeighbors[k]);
                    }
                    if (unownedNeighbors.Count == 0){
                        provinceCrawl.Enqueue(RandomUnownedProvince());
                    } else {
                        foreach (Land neighbor in unownedNeighbors){
                            provinceCrawl.Enqueue(neighbor);
                        }
                    }
                }
            }
            // TODO: Prefer coast
            Land RandomUnownedProvince() => unownedLandProvinces.ElementAt(Random.Range(0, unownedLandProvinces.Count));
        }
    /*
        private void OnDrawGizmos(){
            foreach ((Color color, ProvinceGenerator provinceGenerator) in provinceGenerators){
                Handles.color = color;
                provinceGenerator.GizmosPolygon(ConvertToWorldSpace);
            }
        }*/
    #endif
    }
}