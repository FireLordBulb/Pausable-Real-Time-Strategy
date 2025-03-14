using System.Collections.Generic;
using UnityEngine;

public class MeshData {
	public readonly List<Vector3> Vertices = new();
	public readonly List<Vector3> Normals = new();
	public readonly List<Vector2> UVs = new();
	public readonly List<int> Triangles = new();
	private readonly string name;
	public MeshData(string meshName){
		name = meshName;
	}
	
	public Mesh ToMesh(){
		Mesh mesh = new Mesh {
			name = name,
			vertices = Vertices.ToArray(),
			normals = Normals.ToArray(),
			uv = UVs.ToArray(),
			triangles = Triangles.ToArray(),
			hideFlags = HideFlags.DontSave
		};
		mesh.RecalculateBounds();
		return mesh;
	}
}
