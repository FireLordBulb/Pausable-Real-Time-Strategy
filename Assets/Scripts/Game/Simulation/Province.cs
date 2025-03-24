using System;
using System.Collections.Generic;
using Graphs;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Province : MonoBehaviour, IPositionNode<ProvinceLink, Province> {
    private const float OneThird = 1/3f;
    [SerializeField] private MeshFilter outlineMeshFilter;
    [SerializeField] private MeshFilter shapeMeshFilter;
    [SerializeField] private MeshRenderer shapeMeshRenderer;
    [SerializeField] private Color seaColor;
    
    private MeshCollider meshCollider;
    private readonly Dictionary<Color, ProvinceLink> links = new();
    private Type type;
    private Country owner;
    private Color baseColor;
    private Color hoverColor;
    private Color selectedColor;
    private bool isHovered;
    private bool isSelected;
    
    public Color32 Color {get; private set;}
    public Terrain Terrain {get; private set;}
    public Vector2 MapPosition {get; private set;}
    public Vector3 WorldPosition => transform.position;
    public bool IsSea => type == Type.Sea;
    public bool IsCoast => type == Type.Coast;
    public bool IsLand => type is Type.LandLocked or Type.Coast;
    
    public IEnumerable<ProvinceLink> Links => links.Values;
    public ProvinceLink this[Color color] => links[color];
    
    private void Awake(){
        meshCollider = GetComponent<MeshCollider>();
    }
    public void Init(Color32 color, ProvinceData data, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
        Color = color;
        gameObject.name = $"R: {color.r}, G: {color.g}, B: {color.b}";

        if (data == null){
            baseColor = seaColor;
            type = Type.Sea;
            Terrain = Terrain.Sea;
        } else {
            baseColor = data.Color;
            Terrain = data.Terrain;
            type = Type.LandLocked;
        }
        shapeMeshRenderer.material.color = baseColor;
        UpdateColors();

        MapPosition = mapPosition;
        outlineMeshFilter.sharedMesh = outlineMesh;
        meshCollider.sharedMesh = shapeMesh;
        shapeMeshFilter.sharedMesh = shapeMesh;
    }
    public void AddNeighbor(Province neighbor){
        if (type == Type.LandLocked && neighbor.type == Type.Sea){
            type = Type.Coast;
        }
        links.Add(neighbor.Color, new ProvinceLink(this, neighbor));
    }

    public void SetOwner(Country newOwner){
        if (owner == newOwner){
            return;
        }
        if (owner != null){
            owner.LoseProvince(this);
        }
        owner = newOwner;
        if (owner != null){
            owner.GainProvince(this);
            baseColor = owner.MapColor;
        } else {
            baseColor = UnityEngine.Color.black;
        }
        UpdateColors();
    }
    public bool HasOwner => owner != null;

    private void UpdateColors(){
        float increasedBrightness = OneThird*(baseColor.grayscale+2);
        selectedColor = 0.5f*(baseColor+new Color(increasedBrightness, increasedBrightness, increasedBrightness));
        hoverColor = 0.5f*(baseColor+UnityEngine.Color.gray);
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

[Serializable]
public class ProvinceData {
    [SerializeField] private Color32 color;
    [SerializeField] private Terrain terrain;
    public Color32 Color => color;
    public Terrain Terrain => terrain;
}
