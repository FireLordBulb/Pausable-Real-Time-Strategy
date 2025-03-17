using System.Collections.Generic;
using Collections;
using LoopList = Collections.LinkedLoopList<UnityEngine.Vector2>;
using Node = Collections.LinkedLoopList<UnityEngine.Vector2>.Node<UnityEngine.Vector2>;
using Mathematics;
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

	private readonly float borderHalfWidth;

	private Vector2 min, max;
	
	public ProvinceGenerator(float borderWidth){
		borderHalfWidth = borderWidth;
	}
	
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
		min = Vector2.positiveInfinity;
		max = Vector2.negativeInfinity;
		foreach (Vector2 vertex in vertices){
			for (int i = 0; i < 2 /*The 2 of Vector2*/; i++){
				if (max[i] < vertex[i]){
					max[i] = vertex[i];
				} else if (min[i] > vertex[i]){
					min[i] = vertex[i];
				}
			}
		}
		Center = (min+max)/2;
		// TODO: ensure center is within the convex polygon.
		for (int i = 0; i < vertices.Count; i++){
			vertices[i] -= Center;
		}
	}
	
	private void GenerateOutlineMesh(){
		MeshData meshData = new("ProvinceOutline");
		Vector2 beforeStart = vertices[^1];
		Vector2 start = vertices[0];
		for (int i = 1; i <= vertices.Count; i++){
			Vector2 end = vertices[i%vertices.Count];

			Vector2 beforePerpendicular = VectorGeometry.LeftPerpendicular(beforeStart, start).normalized;
			Vector2 middlePerpendicular = VectorGeometry.LeftPerpendicular(start, end).normalized;
			
			Vector2 offset = (beforePerpendicular+middlePerpendicular).normalized;
			offset *= borderHalfWidth/Vector2.Dot(offset, beforePerpendicular);
			AddBorderSection(meshData, start+offset, start-offset);

			beforeStart = start;
			start = end;
		}
		// Make vertex indices in the last triangles point loop around to the first pair of vertices.
		meshData.Triangles[^4] %= meshData.Vertices.Count;
		meshData.Triangles[^2] %= meshData.Vertices.Count;
		meshData.Triangles[^1] %= meshData.Vertices.Count;
		OutlineMesh = meshData.ToMesh();
	}
	private void AddBorderSection(MeshData meshData, Vector2 left, Vector2 right){
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
	
	private void GenerateShapeMesh(){
		(Vector2, Vector2) lineA = (new Vector2(-2, 10), new Vector2(2, -10));
		(Vector2, Vector2) lineB = (new Vector2(-3, 3), new Vector2(2, -3));
		Debug.Log($"lineA: {lineA}, lineB: {lineB}, do they cross: {DoLineSegmentsCross(lineA, lineB)}");
		
		MeshData meshData = new("ProvinceShape");
		
		/*
		meshData.Vertices.Add(Vector3.zero);
		meshData.Normals.Add(Vector3.up);
		meshData.UVs.Add(GetBoundsUV(Center));

		for (int i = 0; i < vertices.Count; i++){
			meshData.Vertices.Add(VectorGeometry.ToXZPlane(vertices[i]));
			meshData.Normals.Add(Vector3.up);
			meshData.UVs.Add(GetBoundsUV(vertices[i]));
			meshData.Triangles.AddRange(new[]{
				0, i+1, i+2
			});
		}
		// Make the last triangle's corner be the first non-center vertex.
		meshData.Triangles[^1] = 1;
		*/
		for (int i = vertices.Count-1; i >= 0; i--){
			int index = i;
			int otherIndex = (i+1)%vertices.Count;
			Debug.Log(index);
			Vector2 point = vertices[index];
			Vector2 otherPoint = vertices[otherIndex];
			// TODO: Name magic number as a const.
			Vector2 difference = otherPoint-point;
			if (difference.sqrMagnitude <= 2.1f){
				if (otherIndex < index){
					(index, otherIndex) = (otherIndex, index);
				}
				vertices.RemoveAt(otherIndex);
				vertices[index] = 0.5f*(point+otherPoint)+0.5f*VectorGeometry.RightPerpendicular(difference);
			}
		}
		Dictionary<Vector2, int> positionIndexMap = new();
		for (int i = 0; i < vertices.Count; i++){
			meshData.Vertices.Add(VectorGeometry.ToXZPlane(vertices[i]));
			meshData.Normals.Add(Vector3.up);
			meshData.UVs.Add(GetBoundsUV(vertices[i]));
			positionIndexMap.Add(vertices[i], i);
		}
		AddPolygon(new LoopList(vertices), vertices.Count, meshData, positionIndexMap);
		ShapeMesh = meshData.ToMesh();
	}
	private void AddPolygon(LoopList vertexLoop, int length, MeshData meshData, Dictionary<Vector2, int> positionIndexMap){
		Debug.Log("Polygon:");
		foreach (Node node in vertexLoop.First.LoopFrom){
			Debug.Log($"{positionIndexMap[node.Value]}, {node.Value}");
		}
		if (length <= 3){
			Node nodeA = vertexLoop.First;
			Node nodeB = nodeA.Next;
			Node nodeC = nodeB.Next;

			// Swap B and C if triangle is facing down.
			if (Vector3.Cross(VectorGeometry.ToXZPlane(nodeA.Value-nodeB.Value), VectorGeometry.ToXZPlane(nodeB.Value-nodeC.Value)).y < 0){
				(nodeB, nodeC) = (nodeC, nodeB);
			}
			
			meshData.Triangles.Add(positionIndexMap[nodeA.Value]);
			meshData.Triangles.Add(positionIndexMap[nodeB.Value]);
			meshData.Triangles.Add(positionIndexMap[nodeC.Value]);

			
			return;
		}
		
		Node beforeStart = vertexLoop.Last;
		Node start = vertexLoop.First;
		Node halfWayPoint = start;
		for (int i = length/2; i > 0; i--){
			halfWayPoint = halfWayPoint.Next;
		}
		bool isLineFullyInsidePolygon;
		do {
			Vector2 middleDirection = halfWayPoint.Value - start.Value;
			Vector2 leftDirection   = start.Next.Value   - start.Value;
			Vector2 rightDirection  = beforeStart.Value  - start.Value;
			isLineFullyInsidePolygon = VectorGeometry.IsBetweenDirections(middleDirection, leftDirection, rightDirection);
			if (!isLineFullyInsidePolygon){
				beforeStart = start;
				start = start.Next;
				halfWayPoint = halfWayPoint.Next;
				continue;
			}
			foreach (Node node in vertexLoop.First.Next.Next.LoopFrom){
				if (node.Next == vertexLoop.First || crossCheckCount > 10000){
					break;
				}
				if (!DoLineSegmentsCross((start.Value, halfWayPoint.Value), (node.Value, node.Next.Value))){
					continue;
				}
				isLineFullyInsidePolygon = false;
				beforeStart = start;
				start = start.Next;
				halfWayPoint = halfWayPoint.Next;
				break;
			}
		} while (!isLineFullyInsidePolygon && crossCheckCount > 10000);
	
		(LoopList left, LinkedLoopList<Vector2> right) halfLoops = vertexLoop.Split(beforeStart, start, halfWayPoint);
		// Go ahead garbage collector.
		vertexLoop = null;
		AddPolygon(halfLoops.left , (length+1)/2+1, meshData, positionIndexMap);
		// Collect away.
		halfLoops.left = null;
		AddPolygon(halfLoops.right, (length)/2+1  , meshData, positionIndexMap);
	}
	private int crossCheckCount;
	private bool DoLineSegmentsCross((Vector2 a, Vector2 b) firstLine, (Vector2 a, Vector2 b) secondLine){
		crossCheckCount++;
		Vector2 firstDifference = firstLine.b-firstLine.a;
		// TODO: Handle firstDifference.x = 0 case.
		if (Mathf.Abs(firstDifference.x) < Vector2.kEpsilon){
			firstDifference = VectorGeometry.Swizzle(firstDifference);
			firstLine .a = VectorGeometry.Swizzle(firstLine .a);
			firstLine .b = VectorGeometry.Swizzle(firstLine .b);
			secondLine.a = VectorGeometry.Swizzle(secondLine.a);
			secondLine.b = VectorGeometry.Swizzle(secondLine.b);
		}
		float firstEquationSlope = firstDifference.y/firstDifference.x;
		float firstEquationConstant = firstLine.a.y-firstEquationSlope*firstLine.a.x;
		
		Vector2 secondDifference = secondLine.b-secondLine.a;
		float secondEquationSlope = secondDifference.y/secondDifference.x;
		float secondEquationConstant = secondLine.a.y-secondEquationSlope*secondLine.a.x;

		if (Mathf.Abs(firstEquationSlope-secondEquationSlope) < Vector2.kEpsilon){
			return false;
		}
		// secondEquationConstant+ x*secondEquationSlope =firstEquationConstant+ x*firstEquationSlope 
		Vector2 intersection = new((secondEquationConstant-firstEquationConstant)/(firstEquationSlope-secondEquationSlope), 0);
		intersection.y = firstEquationSlope*intersection.x + firstEquationConstant;

		Debug.Log($"intersection: {intersection}");
		return (firstLine .a.x < intersection.x && intersection.x < firstLine .b.x || firstLine .b.x < intersection.x && intersection.x < firstLine .a.x) &&
		       (secondLine.a.x < intersection.x && intersection.x < secondLine.b.x || secondLine.b.x < intersection.x && intersection.x < secondLine.a.x) &&
		       (firstLine .a.y < intersection.y && intersection.y < firstLine .b.y || firstLine .b.y < intersection.y && intersection.y < firstLine .a.y) &&
		       (secondLine.a.y < intersection.y && intersection.y < secondLine.b.y || secondLine.b.y < intersection.y && intersection.y < secondLine.a.y);
	}
	private Vector2 GetBoundsUV(Vector2 position) => new(InverseLerp(position, X), InverseLerp(position, Y));
	private const int X = 0, Y = 1;
	private float InverseLerp(Vector2 vector, int dimension){
		return Mathf.InverseLerp(min[dimension], max[dimension], vector[dimension]);
	}
/*#if UNITY_EDITOR
	public void GizmosPolygon(Func<Vector2, Vector3> worldSpaceConverter){
		for (int i = 0; i < vertices.Count; i++){
			Handles.DrawLine(worldSpaceConverter(Center+vertices[i]), worldSpaceConverter(Center+vertices[(i+1)%vertices.Count]));
		}
	}
#endif*/
}