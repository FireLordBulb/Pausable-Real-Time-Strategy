using System.Collections.Generic;
using Mathematics;
using UnityEngine;

namespace ProceduralMeshes {
	public static class PolygonOutline {
		private static readonly Vector2 HalfUp = new(0, 0.5f);
		public static void GenerateMeshData(MeshData meshData, List<Vector2> vertices, float halfWidth, bool isHalfWide = false){
			if (vertices.Count == 0){
				return;
			}
			int startIndex = meshData.Vertices.Count;
			Vector2 beforeStart = vertices[^1];
			Vector2 start = vertices[0];
			for (int i = 1; i <= vertices.Count; i++){
				Vector2 end = vertices[i%vertices.Count];
				// Skip if two adjacent vertices are identical to avoid a division by zero and it won't change visually anyway. 
				if (beforeStart != start){
					Vector2 beforePerpendicular = VectorGeometry.LeftPerpendicular(beforeStart, start).normalized;
					Vector2 middlePerpendicular = VectorGeometry.LeftPerpendicular(start, end).normalized;
				
					Vector2 offset = (beforePerpendicular+middlePerpendicular).normalized;
					offset *= halfWidth/Vector2.Dot(offset, beforePerpendicular);
					AddBorderSection(meshData, isHalfWide ? start : start+offset, start-offset, isHalfWide ? HalfUp : Vector2.up);
				}
				beforeStart = start;
				start = end;
			}
			// Make vertex indices in the last triangles point loop around to the first pair of vertices.
			meshData.Triangles[^4] = startIndex+0;
			meshData.Triangles[^2] = startIndex+0;
			meshData.Triangles[^1] = startIndex+1;
		}
		private static void AddBorderSection(MeshData meshData, Vector2 left, Vector2 right, Vector2 leftUV){
			int startIndex = meshData.Vertices.Count;
			meshData.Vertices.Add(VectorGeometry.ToXZPlane(left));
			meshData.Vertices.Add(VectorGeometry.ToXZPlane(right));
			meshData.Normals.Add(Vector3.up);
			meshData.Normals.Add(Vector3.up);
			meshData.UVs.Add(leftUV);
			meshData.UVs.Add(Vector2.zero);
			meshData.Triangles.AddRange(new[]{
				startIndex+1, startIndex+0, startIndex+2,
				startIndex+1, startIndex+2, startIndex+3
			});
		}
	}
}