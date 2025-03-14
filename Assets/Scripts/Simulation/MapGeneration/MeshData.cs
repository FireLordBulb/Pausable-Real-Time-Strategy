using System.Collections.Generic;
using UnityEngine;

public class MeshData {
	public readonly List<Vector3> Vertices = new();
	public readonly List<Vector3> Normals = new();
	public readonly List<Vector2> UVs = new();
	public readonly List<int> Triangles = new();

	public Mesh ToMesh(){
		Mesh mesh = new Mesh {
			vertices = Vertices.ToArray(),
			normals = Normals.ToArray(),
			uv = UVs.ToArray(),
			triangles = Triangles.ToArray()
		};
		mesh.RecalculateBounds();
		return mesh;
	}
}
