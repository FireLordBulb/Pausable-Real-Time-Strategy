using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProvinceGenerator {
	private static readonly Vector2 HalfPixel = new(0.5f, 0.5f);
	// Only handles one quadrant. Rotate the input so it matches the quadrant and then rotate the output offset the same amount in reverse.
	private static readonly Dictionary<(bool isDiagonal, Vector2Int directionToNext), Vector2> OuterEdgeOffsets = GetOuterEdgeOffsets();
	private static Dictionary<(bool, Vector2Int), Vector2> GetOuterEdgeOffsets(){
		Dictionary<(bool, Vector2Int), Vector2> outerEdgeOffsets = new();
		
		outerEdgeOffsets.Add((false, Vector2Int.left         ), new Vector2(+0.5f, 0    ));
		outerEdgeOffsets.Add((false, VectorGeometry.UpLeft   ), new Vector2(+0.5f, -0.5f));
		// No 90 degree left turns, so Vector2Int.up is skipped
		outerEdgeOffsets.Add((false, VectorGeometry.UpRight  ), new Vector2(0    , +0.5f));
		// No vertices along 180 degree straight lines, so Vector2Int.right is skipped
		outerEdgeOffsets.Add((false, VectorGeometry.DownRight), new Vector2(0    , +0.5f));
		outerEdgeOffsets.Add((false, Vector2Int.down         ), new Vector2(+0.5f, +0.5f));
		outerEdgeOffsets.Add((false, VectorGeometry.DownLeft ), new Vector2(+1.0f, +0.5f));

		outerEdgeOffsets.Add((true , Vector2Int.left         ), new Vector2(+0.5f, +0.5f));
		outerEdgeOffsets.Add((true , VectorGeometry.UpLeft   ), new Vector2(-0.5f, 0    ));
		outerEdgeOffsets.Add((true , Vector2Int.up           ), new Vector2(-0.5f, 0    ));
		// No vertices along 180 degree straight lines, so VectorGeometry.UpRight is skipped
		outerEdgeOffsets.Add((true , Vector2Int.right        ), new Vector2(0    , +0.5f));
		outerEdgeOffsets.Add((true , VectorGeometry.DownRight), new Vector2(0    , +0.5f));
		outerEdgeOffsets.Add((true , Vector2Int.down         ), new Vector2(+0.5f, +1.0f));
		// The outline can not reach a dead end along diagonals, VectorGeometry.DownLeft is skipped.
		
		return outerEdgeOffsets;
	}
	
	
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
				AddEdgeVertex(currentPixel, differenceFromPrevious, differenceToNext);
			}
			
			previousPixel = currentPixel;
			currentPixel = nextPixel;
		}
	}
	private void AddEdgeVertex(Vector2Int pixel, Vector2Int directionFromPrevious, Vector2Int directionToNext){
		// Aad a half pixel to convert the bottom left of the pixel to the center.
		Vector2 pixelCenter = pixel+HalfPixel;
				
		// Rotate everything by 90 degrees until differenceFromPrevious is in the correct quadrant.
		for (int i = 0; i < MapGenerator.CardinalDirections.Length; i++){
			bool isDiagonal = directionFromPrevious == VectorGeometry.UpRight;
			if (directionFromPrevious == Vector2Int.right || isDiagonal){
				Vector2 outerEdgeOffset = OuterEdgeOffsets[(isDiagonal, directionToNext)];
				// Rotate in reverse the same number of 90 degree turns to get back to world space.
				for (int j = i; j > 0; j--){
					outerEdgeOffset = VectorGeometry.RightPerpendicular(outerEdgeOffset);
				}
				vertices.Add(pixelCenter+outerEdgeOffset);
				return;
			}
			// Rotates both directions by 90 degrees counter-clockwise.
			directionFromPrevious = VectorGeometry.LeftPerpendicular(directionFromPrevious);
			directionToNext = VectorGeometry.LeftPerpendicular(directionToNext);
		}
	}
	private void CalculateCenter(){
		Center = vertices[0];
		// TODO
	}
	private void GenerateOutlineMesh(){
		OutlineMesh = new Mesh();
		// TODO
	}
	private void GenerateShapeMesh(){
		ShapeMesh = new Mesh();
		// TODO
	}
#if UNITY_EDITOR
	public void GizmosPolygon(Vector2 offset, float scale){
		for (int i = 0; i < vertices.Count; i++){
			Handles.DrawLine(ConvertToWorldSpace(vertices[i]), ConvertToWorldSpace(vertices[(i+1)%vertices.Count]));
		}
		Vector3 ConvertToWorldSpace(Vector2 vector){
			return VectorGeometry.ToXZPlane((vector)*scale + offset);
		}
	}
#endif
}