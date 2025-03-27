using UnityEngine;

public static class Format {
	private const float Cent = 100f;

	public static string SignedPercent(float value){
		char sign = value < 0 ? '-' : '+';
		return $"{sign}{Mathf.RoundToInt(Mathf.Abs(value)*Cent)}%";
	}
}
