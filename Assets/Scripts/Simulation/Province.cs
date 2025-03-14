using UnityEngine;

public class Province : MonoBehaviour {
    [SerializeField] private MeshFilter outlineMeshFilter;
    [SerializeField] private MeshFilter shapeMeshFilter;
    [SerializeField] private MeshRenderer shapeMeshRenderer;
    
    public Color Color {get; private set;}

    public void Init(Color color, Mesh outlineMesh, Mesh shapeMesh){
        Color = color;
        outlineMeshFilter.sharedMesh = outlineMesh;
        shapeMeshFilter.sharedMesh = shapeMesh;
        shapeMeshRenderer.material.color = color;
    }
}
