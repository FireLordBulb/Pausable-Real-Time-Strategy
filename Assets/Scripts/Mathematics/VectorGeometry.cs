using UnityEngine;

public static class VectorGeometry {
	public static Vector3 ToXZPlane(Vector2 vector) => new(vector.x, 0, vector.y);
}
