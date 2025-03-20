using UnityEngine;

namespace Mathematics {
	public static class VectorGeometry {
		public static Vector3 ToXZPlane(Vector2 vector) => new(vector.x, 0, vector.y);

		public static readonly Vector2Int UpRight = new(+1, +1);
		public static readonly Vector2Int DownRight = new(+1, -1);
		public static readonly Vector2Int DownLeft = new(-1, -1);
		public static readonly Vector2Int UpLeft = new(-1, +1);
		
		public static Vector2Int LeftPerpendicular(Vector2Int vector) => new(-vector.y, vector.x);
		public static Vector2Int RightPerpendicular(Vector2Int vector) => new(vector.y, -vector.x);
		
		public static bool DoLineSegmentsCross((Vector2 a, Vector2 b) firstLine, (Vector2 a, Vector2 b) secondLine){
			Vector2 firstDifference  = firstLine .b-firstLine .a;
			Vector2 secondDifference = secondLine.b-secondLine.a;

			bool isFirstVertical  = Mathf.Abs(firstDifference .x) < Vector2.kEpsilon;
			bool isSecondVertical = Mathf.Abs(secondDifference.x) < Vector2.kEpsilon;
			if (isFirstVertical && isSecondVertical){
				return false;
			}
			if (isFirstVertical ^ isSecondVertical){
				firstDifference  = Swizzle(firstDifference );
				secondDifference = Swizzle(secondDifference);
				firstLine .a = Swizzle(firstLine .a);
				firstLine .b = Swizzle(firstLine .b);
				secondLine.a = Swizzle(secondLine.a);
				secondLine.b = Swizzle(secondLine.b);
			}
		
			float firstEquationSlope  = firstDifference .y/firstDifference .x;
			float secondEquationSlope = secondDifference.y/secondDifference.x;
			if (Mathf.Abs(firstEquationSlope-secondEquationSlope) < Vector2.kEpsilon){
				return false;
			}
		
			float firstEquationConstant  = firstLine .a.y -   firstEquationSlope*firstLine.a.x;
			float secondEquationConstant = secondLine.a.y - secondEquationSlope*secondLine.a.x;

			float intersectionX = (secondEquationConstant-firstEquationConstant)/(firstEquationSlope-secondEquationSlope);
			float intersectionMarginUp   = intersectionX+4*Vector2.kEpsilon;
			float intersectionMarginDown = intersectionX-4*Vector2.kEpsilon;
			return IsIntersectionOnSegment(firstLine) && IsIntersectionOnSegment(secondLine);

			bool IsIntersectionOnSegment((Vector2 a, Vector2 b) lineSegment){
				return IsIntersectionInRange(lineSegment.a.x, lineSegment.b.x) || IsIntersectionInRange(lineSegment.b.x, lineSegment.a.x);
			}
			bool IsIntersectionInRange(float lower, float upper){
				return lower <= intersectionMarginUp && intersectionMarginDown <= upper;
			}
		}
		
		public static bool IsBetweenDirections(Vector2 middleDirection, Vector2 leftDirection, Vector2 rightDirection){
			if (IsClockwise(leftDirection, rightDirection)){
				return IsClockwise(leftDirection, middleDirection) &&
				       IsClockwise(middleDirection, rightDirection);
			}
			return IsClockwise(leftDirection, middleDirection) ||
			       IsClockwise(middleDirection, rightDirection);
			
		}
		public static bool IsClockwise(Vector2 leftDirection, Vector2 rightDirection){
			return Vector2.Dot(leftDirection, LeftPerpendicular(rightDirection)) > 0;
		}

		public static Vector2 LeftPerpendicular(Vector2 start, Vector2 end) => LeftPerpendicular((end-start));
		public static Vector2 LeftPerpendicular(Vector2 vector) => new(-vector.y, vector.x);
		public static Vector2 RightPerpendicular(Vector2 vector) => new(vector.y, -vector.x);
		
		public static Vector2 Swizzle(Vector2 vector) => new(vector.y, vector.x);
	}
}