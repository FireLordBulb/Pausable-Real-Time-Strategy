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

		public static bool IsBetweenDirections(Vector2 middleDirection, Vector2 leftDirection, Vector2 rightDirection){
			bool isAcuteInnerAngle = Vector2.Dot(leftDirection, LeftPerpendicular(rightDirection)) > 0;
			if (isAcuteInnerAngle){
				return Vector2.Dot(middleDirection, RightPerpendicular(leftDirection)) > 0 &&
				       Vector2.Dot(middleDirection, LeftPerpendicular(rightDirection)) > 0;
			}
			return Vector2.Dot(middleDirection, RightPerpendicular(leftDirection)) > 0 ||
			       Vector2.Dot(middleDirection, LeftPerpendicular(rightDirection)) > 0;
			
		}

		public static Vector2 LeftPerpendicular(Vector2 start, Vector2 end) => LeftPerpendicular((end-start));
		public static Vector2 LeftPerpendicular(Vector2 vector) => new(-vector.y, vector.x);
		public static Vector2 RightPerpendicular(Vector2 vector) => new(vector.y, -vector.x);
		
		public static Vector2 Swizzle(Vector2 vector) => new(vector.y, vector.x);
	}
}