using System.Collections.Generic;
using Mathematics;
using UnityEngine;

namespace ProceduralMeshes {
	public class PolygonOutline {
		public static void GenerateMeshData(MeshData meshData, List<Vector2> vertices, float halfWidth){
			Vector2 beforeStart = vertices[^1];
			Vector2 start = vertices[0];
			for (int i = 1; i <= vertices.Count; i++){
				Vector2 end = vertices[i%vertices.Count];

				Vector2 beforePerpendicular = VectorGeometry.LeftPerpendicular(beforeStart, start).normalized;
				Vector2 middlePerpendicular = VectorGeometry.LeftPerpendicular(start, end).normalized;
			
				Vector2 offset = (beforePerpendicular+middlePerpendicular).normalized;
				offset *= halfWidth/Vector2.Dot(offset, beforePerpendicular);
				AddBorderSection(meshData, start+offset, start-offset);

				beforeStart = start;
				start = end;
			}
			// Make vertex indices in the last triangles point loop around to the first pair of vertices.
			meshData.Triangles[^4] %= meshData.Vertices.Count;
			meshData.Triangles[^2] %= meshData.Vertices.Count;
			meshData.Triangles[^1] %= meshData.Vertices.Count;
		}
		private static void AddBorderSection(MeshData meshData, Vector2 left, Vector2 right){
			int startIndex = meshData.Vertices.Count;
			meshData.Vertices.Add(VectorGeometry.ToXZPlane(left));
			meshData.Vertices.Add(VectorGeometry.ToXZPlane(right));
			meshData.Normals.Add(Vector3.up);
			meshData.Normals.Add(Vector3.up);
			meshData.UVs.Add(Vector2.up);
			meshData.UVs.Add(Vector2.zero);
			meshData.Triangles.AddRange(new[]{
				startIndex+1, startIndex+0, startIndex+2,
				startIndex+1, startIndex+2, startIndex+3
			});
		}
	}
}