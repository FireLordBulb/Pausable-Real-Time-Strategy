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
    
    private MeshCollider meshCollider;
    private readonly Dictionary<Color, ProvinceLink> links = new();
    private Color baseColor;
    private Color hoverColor;
    private Color selectedColor;
    private bool isSelected;
    
    public Color32 Color {get; private set;}
    public Vector2 MapPosition {get; private set;}
    public Vector3 WorldPosition => transform.position;
    
    public IEnumerable<ProvinceLink> Links => links.Values;
    public ProvinceLink this[Color color] => links[color];
    
    private void Awake(){
        meshCollider = GetComponent<MeshCollider>();
    }
    public void Init(Color32 color, ProvinceData data, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
        Color = color;
        baseColor = (Color?)data?.Color ?? UnityEngine.Color.blue;
        float increasedBrightness = OneThird*(baseColor.grayscale+2);
        selectedColor = 0.5f*(baseColor+new Color(increasedBrightness, increasedBrightness, increasedBrightness));
        hoverColor = 0.5f*(baseColor+UnityEngine.Color.gray);
        gameObject.name = $"R: {color.r}, G: {color.g}, B: {color.b}";
        MapPosition = mapPosition;
        outlineMeshFilter.sharedMesh = outlineMesh;
        meshCollider.sharedMesh = shapeMesh;
        shapeMeshFilter.sharedMesh = shapeMesh;
        shapeMeshRenderer.material.color = baseColor;
    }
    public void AddNeighbor(Province neighbor){
        links.Add(neighbor.Color, new ProvinceLink(this, neighbor));
    }

    public void OnHoverEnter(){
        if (!isSelected){
            shapeMeshRenderer.sharedMaterial.color = hoverColor;
        }
    }
    public void OnHoverLeave(){
        if (!isSelected){
            shapeMeshRenderer.sharedMaterial.color = baseColor;
        }
    }
    public void OnSelect(){
        isSelected = true;
        shapeMeshRenderer.sharedMaterial.color = selectedColor;
    }
    public void OnDeselect(){
        isSelected = false;
        shapeMeshRenderer.sharedMaterial.color = baseColor;
    }
}

[Serializable]
public class ProvinceData {
    [SerializeField] private Color32 color;
    public Color32 Color => color;
}
