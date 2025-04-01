using System.Globalization;
using System.Text;
using UnityEngine;

namespace Player {
	public static class Format {
		public const float FiveDigits = 10000f;
		public const float SevenDigits = 1000000f;
		private const float InverseOneK = 0.001f;
		private const int OneK = 1000;
		private const float Cent = 100f;

		public static string FormatLargeNumber(float number, float digitLimit){
			if (number < digitLimit){
				return number.ToString("0.0", CultureInfo.InvariantCulture);
			} 
			StringBuilder builder = new((number*InverseOneK).ToString("0.0", CultureInfo.InvariantCulture));
			builder.Append('k');
			return builder.ToString();
		}
		public static string FormatLargeNumber(int number, float digitLimit){
			if (number < digitLimit){
				return number.ToString();
			} 
			StringBuilder builder = new((number/OneK).ToString());
			builder.Append('k');
			return builder.ToString();
		}
		
		public static string SignedPercent(float value){
			char sign = value < 0 ? '-' : '+';
			return $"{sign}{Mathf.RoundToInt(Mathf.Abs(value)*Cent)}%";
		}
	}
}