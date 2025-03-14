using System;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Province : MonoBehaviour {
    [SerializeField] private MeshFilter outlineMeshFilter;
    [SerializeField] private MeshFilter shapeMeshFilter;
    [SerializeField] private MeshRenderer shapeMeshRenderer;

    private MeshCollider meshCollider;
    
    public Color Color {get; private set;}
    private void Awake(){
        meshCollider = GetComponent<MeshCollider>();
    }
    public void Init(Color color, Mesh outlineMesh, Mesh shapeMesh){
        Color = color;
        outlineMeshFilter.sharedMesh = outlineMesh;
        meshCollider.sharedMesh = shapeMesh;
        shapeMeshFilter.sharedMesh = shapeMesh;
        shapeMeshRenderer.material.color = color;
    }
}
