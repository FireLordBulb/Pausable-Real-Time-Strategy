using System.Globalization;
using UnityEngine;

namespace Player {
	public static class Format {
		private const int SuffixDigitStep = 3;
		private static readonly (char, long)[] Suffixes ={
			('k', 1000),
			('M', 1000000),
			('B', 1000000000),
			('T', 1000000000000),
			('P', 1000000000000000),
			('E', 1000000000000000000)
		};
		private const float Cent = 100f;
		
		public static string FormatLargeNumber(float number, int maxCharacters){
			string untruncated = FloatToString(number);
			int characterAmount = untruncated.Length;
			if (characterAmount <= maxCharacters){
				return untruncated;
			}
			(char character, long value) = GetSuffix(characterAmount, maxCharacters);
			return $"{FloatToString(number/value)}{character}";
		}
		public static string FormatLargeNumber(int number, int maxCharacters){
			string untruncated = number.ToString();
			int characterAmount = untruncated.Length;
			if (characterAmount <= maxCharacters){
				return untruncated;
			}
			(char character, long value) = GetSuffix(characterAmount, maxCharacters);
			return $"{(number/value).ToString()}{character}";
		}
		private static (char, long) GetSuffix(int characterAmount, int maxCharacters){
			foreach ((char, long) suffix in Suffixes){
				characterAmount -= SuffixDigitStep;
				if (characterAmount < maxCharacters){
					return suffix;
				}
			}
			// In case no suffix works just use the largest one.
			return Suffixes[^1];
		}
		private static string FloatToString(float number){
			return number.ToString("0.0", CultureInfo.InvariantCulture);
		}
		
		public static string SignedPercent(float value){
			return $"{Signed(Mathf.RoundToInt(value*Cent))}%";
		}
		public static string Signed(int value){
			char sign = value < 0 ? '-' : '+';
			return $"{sign}{Mathf.Abs(value)}";
		}
	}
}