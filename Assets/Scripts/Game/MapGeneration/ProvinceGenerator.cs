using System.Collections.Generic;
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
		RemoveDoubleCorners();
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

	private void RemoveDoubleCorners(){
		for (int i = vertices.Count-1; i >= 0; i--){
			int index = i;
			int otherIndex = (i+1)%vertices.Count;
			Vector2 point = vertices[index];
			Vector2 otherPoint = vertices[otherIndex];
			Vector2 difference = otherPoint-point;
			if (Vector2.kEpsilon < Mathf.Abs(difference.sqrMagnitude-HalfPixel.sqrMagnitude)){
				continue;
			}
			Vector2 beforePoint = vertices[(i-1+vertices.Count)%vertices.Count];
			Vector2 afterPoint = vertices[(i+2)%vertices.Count];
			if (Vector2.kEpsilon < Mathf.Abs(Vector2.Dot(beforePoint-point, otherPoint-afterPoint))){
				continue;
			}
			if (otherIndex < index){
				(index, otherIndex) = (otherIndex, index);
			}
			vertices.RemoveAt(otherIndex);
			vertices[index] = 0.5f*(point+otherPoint+VectorGeometry.RightPerpendicular(difference));
		}
	}
	
	private void CalculateCenter(){
		min = Vector2.positiveInfinity;
		max = Vector2.negativeInfinity;
		foreach (Vector2 vertex in vertices){
			for (int i = 0; i < 2 /*The 2 of Vector2*/; i++){
				if (max[i] < vertex[i]){
					max[i] = vertex[i];
				}
				if (min[i] > vertex[i]){
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
		MeshData meshData = new("ProvinceShape");
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
	private static void AddPolygon(LoopList vertexLoop, int length, MeshData meshData, Dictionary<Vector2, int> positionIndexMap){
		if (length <= 3){
			Node nodeA = vertexLoop.First;
			Node nodeB = nodeA.Next;
			Node nodeC = nodeB.Next;
			meshData.Triangles.Add(positionIndexMap[nodeA.Value]);
			meshData.Triangles.Add(positionIndexMap[nodeB.Value]);
			meshData.Triangles.Add(positionIndexMap[nodeC.Value]);
			return;
		}

		(NodePair start, NodePair end, int halfLoopSizeOffset, bool wasSuccess) = GetLoopSplittingLine(vertexLoop, length);
		if (!wasSuccess){
			return;
		}
		
		(LoopList a, LoopList b) halfLoops = vertexLoop.Split(start.Previous, start.Main, end.Main);
		AddPolygon(halfLoops.a, (length  )/2+1-halfLoopSizeOffset, meshData, positionIndexMap);
		AddPolygon(halfLoops.b, (length+1)/2+1+halfLoopSizeOffset, meshData, positionIndexMap);
	}
	private static (NodePair start, NodePair end, int halfLoopSizeOffset, bool wasSuccess) GetLoopSplittingLine(LoopList vertexLoop, int length){
		NodePair start = new(vertexLoop.Last);
		NodePair end = start;
		for (int i = length/2; i > 0; i--){
			end.ToNext();
		}
		
		int halfLoopSizeOffset = 0;
		while (true){
			Vector2 direction = end.Main.Value - start.Main.Value;
			bool isLineValid = IsDirectionPointingInwards(direction, start) && IsDirectionPointingInwards(-direction, end) &&
			                   DoesLineCrossNoEdge((start.Main.Value, end.Main.Value), end.Main, start.Next.LoopUntilNextIs(start.Previous));
			if (isLineValid){
				break;
			}
			start.ToNext();
			end.ToNext();
			// If all split lines have been checked with no luck, check the split lines with a vertex less between start and "half"WayPoint.
			if (start.Main != vertexLoop.First){
				continue;
			}
			halfLoopSizeOffset++;
			start.ToNext();
			if (start.Next == end.Main){
				Debug.LogError("Couldn't split loop properly!");
				return (start, end, halfLoopSizeOffset, false);
			}
		}
		return (start, end, halfLoopSizeOffset, true);
	}
	private static bool IsDirectionPointingInwards(Vector2 direction, NodePair nodePair){
		Vector2 leftDirection  = nodePair.Next.Value     - nodePair.Main.Value;
		Vector2 rightDirection = nodePair.Previous.Value - nodePair.Main.Value;
		return VectorGeometry.IsBetweenDirections(direction, leftDirection, rightDirection);
	}
	private static bool DoesLineCrossNoEdge((Vector2, Vector2) line, Node endNode, IEnumerable<Node> edgeNodes){
		foreach (Node node in edgeNodes){
			// The edges that share a vertex with the line will always "cross" it exactly at that vertex, so those edges are skipped.
			if (node == endNode || node.Next == endNode){
				continue;
			}
			if (VectorGeometry.DoLineSegmentsCross(line, (node.Value, node.Next.Value))){
				return false;
			}
		}
		return true;
	}
	private Vector2 GetBoundsUV(Vector2 position) => new(InverseLerpWithinBounds(position, X), InverseLerpWithinBounds(position, Y));
	private const int X = 0, Y = 1;
	private float InverseLerpWithinBounds(Vector2 vector, int dimension){
		return Mathf.InverseLerp(min[dimension], max[dimension], vector[dimension]);
	}
/*
#if UNITY_EDITOR
	public void GizmosPolygon(Func<Vector2, Vector3> worldSpaceConverter){
		for (int i = 0; i < vertices.Count; i++){
			Handles.DrawLine(worldSpaceConverter(Center+vertices[i]), worldSpaceConverter(Center+vertices[(i+1)%vertices.Count]));
		}
	}
#endif*/

	// Essentially a wrapper around Node that allows a node to keep track of its previous node in the LinkedLoopList.
	// This allows the previous node to be accessed without having to make the entire LinkedLoopList bi-directional.
	private struct NodePair {
		public Node Previous {get; private set;}
		public Node Main {get; private set;}
		public Node Next => Main.Next;
		public NodePair(Node previous){
			Previous = previous;
			Main = previous.Next;
		}
		public void ToNext(){
			Previous = Main;
			Main = Main.Next;
		}
	}
}