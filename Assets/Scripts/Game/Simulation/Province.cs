using System.Collections.Generic;
using Graphs;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Province : MonoBehaviour, IPositionNode<ProvinceLink, Province> {
    [SerializeField] private MeshFilter outlineMeshFilter;
    [SerializeField] private MeshFilter shapeMeshFilter;
    [SerializeField] private MeshRenderer shapeMeshRenderer;
    
    private MeshCollider meshCollider;
    private readonly Dictionary<Color, ProvinceLink> links = new();

    public Color Color {get; private set;}
    public Vector2 MapPosition {get; private set;}
    public Vector3 WorldPosition => transform.position;
    
    public IEnumerable<ProvinceLink> Links => links.Values;
    public ProvinceLink this[Color color] => links[color];
    
    private void Awake(){
        meshCollider = GetComponent<MeshCollider>();
    }
    public void Init(Color color, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
        Color = color;
        Color32 color32 = color;
        gameObject.name = $"R: {color32.r}, G: {color32.g}, B: {color32.b}";
        MapPosition = mapPosition;
        outlineMeshFilter.sharedMesh = outlineMesh;
        meshCollider.sharedMesh = shapeMesh;
        shapeMeshFilter.sharedMesh = shapeMesh;
        shapeMeshRenderer.material.color = color;
    }
    public void AddNeighbor(Province neighbor){
        links.Add(neighbor.Color, new ProvinceLink(this, neighbor));
    }

    public void OnClick(){
        shapeMeshRenderer.material.color = Color.white;
    }
}
