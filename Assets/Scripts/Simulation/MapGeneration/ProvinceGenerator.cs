using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProvinceGenerator {
	
	public readonly List<Vector2Int> OutlinePixels = new();
	public readonly HashSet<Color> Neighbors = new();

	private readonly List<Vector2> vertices = new();
	
	public Mesh OutlineMesh {get; private set;}
	public Mesh ShapeMesh {get; private set;}
	public Vector2 Center {get; private set;}

	public void GenerateData(){
		if (OutlinePixels.Count <= 1){
			Debug.LogError("Too few pixels in province!");
			return;
		}
		GenerateVertextList();
		CalculateCenter();
		GenerateOutlineMesh();
		GenerateShapeMesh();
	}
	private void GenerateVertextList(){
		Vector2Int previousPixel = OutlinePixels[^1];
		Vector2Int currentPixel = OutlinePixels[0];
		for (int i = 0; i < OutlinePixels.Count; i++){
			Vector2Int nextPixel = OutlinePixels[(i+1)%OutlinePixels.Count];

			Vector2Int differenceFromPrevious = currentPixel-previousPixel;
			Vector2Int differenceToNext = nextPixel-currentPixel;

			// Only add the current pixel if it's not on a straight line between the previous and next.
			if (differenceFromPrevious-differenceToNext != Vector2Int.zero){
				vertices.Add(currentPixel);
			}

			previousPixel = currentPixel;
			currentPixel = nextPixel;
		}
	}
	private void CalculateCenter(){
		Center = vertices[0];
		// TODO
	}
	private void GenerateOutlineMesh(){
		OutlineMesh = new Mesh();
		// TODO (remember: make 90 degree turns soft)
	}
	private void GenerateShapeMesh(){
		ShapeMesh = new Mesh();
		// TODO
	}
#if UNITY_EDITOR
	private static readonly Vector2 HalfPixel = new(0.5f, 0.5f);
	public void GizmosPolygon(Vector2 offset, float scale){
		for (int i = 0; i < vertices.Count; i++){
			Handles.DrawLine(ConvertToWorldSpace(vertices[i]), ConvertToWorldSpace(vertices[(i+1)%vertices.Count]));
		}
		Vector3 ConvertToWorldSpace(Vector2 vector){
			return VectorGeometry.ToXZPlane((vector+HalfPixel)*scale + offset);
		}
	}
#endif
}