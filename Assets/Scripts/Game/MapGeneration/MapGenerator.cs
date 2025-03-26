using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private (Color32, ProvinceGenerator)[] provinceGenerators;
    private readonly Dictionary<Color32, ProvinceData> provinceDataDictionary = new();
    private readonly HashSet<(Province province, List<Color32>)> provinceNeighbors = new();
    
    private int imageWidth, imageHeight;
    private Vector2 worldSpaceOffset;
    private float worldSpaceScale;
    private Vector3 provinceScale;
    
    private void Awake(){
        Land.ClearProvinceList();
        
        mapGraph = GetComponent<MapGraph>();
        mapPixels = mapImage.GetPixels32();
        
        imageWidth = mapImage.width;
        imageHeight = mapImage.height;
        
        worldSpaceScale = mapWidth/imageWidth;
        worldSpaceOffset = -0.5f*new Vector2(mapWidth, mapWidth*imageHeight/imageWidth);
        provinceScale = new Vector3(worldSpaceScale, 1, worldSpaceScale);
        
        FindProvincePositions();
        GenerateProvinceMeshDataParallel();
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
    
    // Multithreading reduced time from 18ms to 11ms when tested on March 20th's provinces.png file.
    private void GenerateProvinceMeshDataParallel(){
        Thread[] threads = new Thread[Environment.ProcessorCount];
        KeyValuePair<Color32, Vector2Int>[] provincePositionArray = provincePositions.ToArray();
        provinceGenerators = new (Color32, ProvinceGenerator)[provincePositionArray.Length];
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
    }
    private ProvinceGenerator FindProvinceOutline(Color32 color, Vector2Int startPosition){
        ProvinceGenerator province = new(borderWidth, textureScale);
        List<Vector2Int> outlinePixels = new();
        List<int> triPointIndices = new();
        List<Color32> neighbors = new();
        outlinePixels.Add(startPosition);
        Vector2Int position = startPosition;
        // You're moving right during the full mapImage iteration, so the up direction (with index 0) is a turn to the left.
        int leftTurnIndex = 0;
        Color32? previousNeighborColor = null;
        Color32? firstNeighborColor = null;
        int steps = 0;
        do {
            for (int i = 0; i < CardinalDirections.Length; i++){
                int directionIndex = (leftTurnIndex+i+CardinalDirections.Length) % CardinalDirections.Length;
                Vector2Int newPosition = position+CardinalDirections[directionIndex];
                if (newPosition.x < 0 || imageWidth <= newPosition.x || newPosition.y < 0 || imageHeight <= newPosition.y){
                    firstNeighborColor ??= OffMapColorKey;
                    if (previousNeighborColor != null && !previousNeighborColor.Equals(OffMapColorKey)){
                        triPointIndices.Add(outlinePixels.Count-1);
                    }
                    previousNeighborColor = OffMapColorKey;
                    continue;
                }
                Color32 newPixelColor = GetPixel(newPosition.x, newPosition.y);
                if (!color.Equals(newPixelColor)){
                    firstNeighborColor ??= newPixelColor;
                    // TODO Fix to remove linear search for FindIndex
                    if (neighbors.FindIndex(c => c.Equals(newPixelColor)) == -1){
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
        } while (outlinePixels[^1] != startPosition && steps < MaxOutlineSteps);

        // Remove the last pixel if it's a duplicate of the first.
        if (outlinePixels[^1] == outlinePixels[0]){
            outlinePixels.RemoveAt(outlinePixels.Count-1);
        }
        // Checking for a tri-point exactly where the loop connects.
        if (!firstNeighborColor.Equals(previousNeighborColor)){
            triPointIndices.Add(outlinePixels.Count-1);
            //triPointIndices.Insert(0, 0);
        }
        
        province.OutlinePixels.AddRange(outlinePixels);
        province.Neighbors.AddRange(neighbors);
        province.TriPointIndices.AddRange(triPointIndices);
        return province;
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
            Vector3 worldPosition = ConvertToWorldSpace(provinceGenerator.Pivot);
            Province province;
            if (provinceDataDictionary.TryGetValue(color, out ProvinceData data)){
                Land land = Instantiate(landPrefab, worldPosition, Quaternion.identity, provinceParent);
                land.Init(color, data, provinceGenerator.Pivot, provinceGenerator.OutlineMesh, provinceGenerator.ShapeMesh);
                province = land.Province;
            } else {
                Sea sea = Instantiate(seaPrefab, worldPosition, Quaternion.identity, provinceParent);
                sea.Init(color, provinceGenerator.Pivot, provinceGenerator.OutlineMesh, provinceGenerator.ShapeMesh);
                province = sea.Province;
            }
            province.transform.localScale = provinceScale;
            provinceNeighbors.Add((province, provinceGenerator.Neighbors));
            mapGraph.Add(province);

            province.TriPointIndices = provinceGenerator.TriPointIndices;
        }
    }

    private void LinkNeighboringProvinces(){
        foreach ((Province province, List<Color32> neighbors) in provinceNeighbors){
            foreach (Color32 neighborColor in neighbors){
                province.AddNeighbor(mapGraph[neighborColor]);
            }
        }
    }
    
    private Color32 GetPixel(int x, int y){
        return mapPixels[x+y*imageWidth];
    }
    
    private Vector3 ConvertToWorldSpace(Vector2 vector){
        return VectorGeometry.ToXZPlane(vector*worldSpaceScale + worldSpaceOffset);
    }

    private void InitializeCountries(){
        foreach (CountryData data in countryData.List){
            Country country = Instantiate(countryPrefab, countryParent);
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
        HashSet<Province> unownedLandProvinces = new();
        foreach (ProvinceData data in provinceDataDictionary.Values){
            Province landProvince = mapGraph[data.Color];
            landProvince.SetOwner(null);
            unownedLandProvinces.Add(landProvince);
        }
        Queue<Province> provinceCrawl = new();
        provinceCrawl.Enqueue(RandomUnownedProvince());
        for (int i = countries.Length-1; i >= 0; i--){
            int countryProvinceCount = Mathf.Max(Mathf.Min(Random.Range(1, maxProvinces+1), landProvincesLeft-i), (landProvincesLeft-i)/(i+1));
            for (int j = 0; j < countryProvinceCount; j++){
                Province province = provinceCrawl.Dequeue();
                province.SetOwner(countries[i]);
                unownedLandProvinces.Remove(province);
                landProvincesLeft--;
                if (provinceCrawl.Count != 0 || landProvincesLeft == 0){
                    continue;
                }
                List<Province> unownedNeighbors = new();
                foreach (ProvinceLink link in province.Links){
                    Province neighbor = link.Target;
                    if (!neighbor.HasOwner && !neighbor.IsSea){
                        unownedNeighbors.Add(neighbor);
                    }
                }
                for (int k = unownedNeighbors.Count-1; k > 0; k--){
                    int randomIndex = Random.Range(0, k+1);
                    (unownedNeighbors[k], unownedNeighbors[randomIndex]) = (unownedNeighbors[randomIndex], unownedNeighbors[k]);
                }
                if (unownedNeighbors.Count == 0){
                    provinceCrawl.Enqueue(RandomUnownedProvince());
                } else {
                    foreach (Province neighbor in unownedNeighbors){
                        provinceCrawl.Enqueue(neighbor);
                    }
                }
            }
        }
        // TODO: Prefer coast
        Province RandomUnownedProvince() => unownedLandProvinces.ElementAt(Random.Range(0, unownedLandProvinces.Count));
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