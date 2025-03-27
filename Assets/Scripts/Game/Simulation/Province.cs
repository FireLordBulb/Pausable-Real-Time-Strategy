using System.Collections.Generic;
using Graphs;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Province : MonoBehaviour, IPositionNode<ProvinceLink, Province> {
    private const float OneThird = 1/3f;
    [SerializeField] private MeshFilter outlineMeshFilter;
    [SerializeField] private MeshFilter shapeMeshFilter;
    [SerializeField] private MeshRenderer shapeMeshRenderer;
    [SerializeField] private MeshFilter terrainMeshFilter;
    [SerializeField] private MeshRenderer terrainMeshRenderer;
    
    private MeshCollider meshCollider;
    private readonly Dictionary<Color32, ProvinceLink> links = new();
    private Type type;
    private Country owner;
    private Color baseColor;
    private Color hoverColor;
    private Color selectedColor;
    private bool isHovered;
    private bool isSelected;
    
    
    public List<int> TriPointIndices;
    public List<Vector2> Vertices;
    public List<(int startIndex, int endIndex, ProvinceLink link)> outlineSegments = new();
    
    
    public Color32 ColorKey {get; private set;}
    public Terrain Terrain {get; private set;}
    public Vector2 MapPosition {get; private set;}
    public Vector3 WorldPosition => transform.position;
    public bool IsSea => type == Type.Sea;
    public bool IsCoast => type == Type.Coast;
    public bool IsLand => type is Type.LandLocked or Type.Coast;
    public bool HasOwner => owner != null;
    
    public IEnumerable<ProvinceLink> Links => links.Values;
    public ProvinceLink this[Color32 color] => links[color];
    
    private void Awake(){
        meshCollider = GetComponent<MeshCollider>();
    }
    public void Init(Color32 colorKey, Type provinceType, Terrain terrain, Color mapColor, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
        ColorKey = colorKey;
        gameObject.name = $"R:{colorKey.r}, G:{colorKey.g}, B:{colorKey.b}";

        type = provinceType;
        Terrain = terrain;
        baseColor = mapColor;
        
        shapeMeshRenderer.material.color = baseColor;
        UpdateColors();

        MapPosition = mapPosition;
        outlineMeshFilter.sharedMesh = outlineMesh;
        meshCollider.sharedMesh = shapeMesh;
        shapeMeshFilter.sharedMesh = shapeMesh;
        terrainMeshFilter.sharedMesh = shapeMesh;
        terrainMeshRenderer.sharedMaterial = terrain.Material;
#if UNITY_EDITOR
        terrainMeshFilter.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
    }
    public void AddNeighbor(Province neighbor, int triPointIndex){
        ProvinceLink newLink;
        if (neighbor != null){
            if (type == Type.LandLocked && neighbor.type == Type.Sea){
                type = Type.Coast;
            }
            newLink = new ProvinceLink(this, neighbor, outlineSegments.Count);
            links.Add(neighbor.ColorKey, newLink);
        } else {
            newLink = null;
        }
        int previousTriPointIndex = outlineSegments.Count == 0 ? -1 : outlineSegments[^1].endIndex;
        outlineSegments.Add((previousTriPointIndex, triPointIndex, newLink));
    }
    public void CompleteSegmentLoop(){
        (int _, int endIndex, ProvinceLink link) = outlineSegments[0];
        outlineSegments[0] = (outlineSegments[^1].endIndex, endIndex, link);
    }

    public void SetOwner(Country newOwner){
        if (owner == newOwner){
            return;
        }
        if (owner != null){
            owner.LoseProvince(this);
        }
        owner = newOwner;
        float alpha = baseColor.a;
        if (owner != null){
            owner.GainProvince(this);
            baseColor = owner.MapColor;
        } else {
            baseColor = Color.black;
        }
        baseColor.a = alpha;

        UpdateColors();
    }
    
    public void SetAlpha(float alpha){
        baseColor.a = alpha;
        UpdateColors();
    }
    private void UpdateColors(){
        float increasedBrightness = OneThird*(baseColor.grayscale+2);
        selectedColor = 0.5f*(baseColor+new Color(increasedBrightness, increasedBrightness, increasedBrightness));
        hoverColor = 0.5f*(baseColor+Color.gray);
        shapeMeshRenderer.sharedMaterial.color = isSelected ? selectedColor : isHovered ? hoverColor : baseColor;
    }
    
    public void OnHoverEnter(){
        if (isSelected){
            return;
        }
        shapeMeshRenderer.sharedMaterial.color = hoverColor;
        isHovered = true;
    }
    public void OnHoverLeave(){
        if (isSelected){
            return;
        }
        shapeMeshRenderer.sharedMaterial.color = baseColor;
        isHovered = false;
    }
    public void OnSelect(){
        isSelected = true;
        shapeMeshRenderer.sharedMaterial.color = selectedColor;
    }
    public void OnDeselect(){
        isSelected = false;
        shapeMeshRenderer.sharedMaterial.color = baseColor;
    }

    public enum Type {
        Sea,
        LandLocked,
        Coast
    }
}
