using System.Globalization;
using System.Text;
using UnityEngine;

namespace Text {
	public static class Format {
		private const int SuffixDigitStep = 3;
		private const int Base = 10;
		private const decimal InverseBase = 1/(decimal)Base;
		private static readonly (char, long)[] Suffixes ={
			('k', 1000),
			('M', 1000000),
			('B', 1000000000),
			('T', 1000000000000),
			('P', 1000000000000000),
			('E', 1000000000000000000)
		};
		private const float Cent = 100f;

		public static string FormatLargeNumberWithSign(double number, int maxCharacters){
			return FormatLargeNumberWithSign((decimal)number, maxCharacters);
		}
		public static string FormatLargeNumberWithSign(decimal number, int maxCharacters){
			return new StringBuilder(number < 0 ? "" : "+").Append(FormatLargeNumber(number, maxCharacters)).ToString();
		}
		public static string FormatLargeNumberWithSign(long number, int maxCharacters){
			return new StringBuilder(number < 0 ? "" : "+").Append(FormatLargeNumber(number, maxCharacters)).ToString();
		}

		public static string FormatLargeNumber(double number, int maxCharacters){
			return FormatLargeNumber((decimal)number, maxCharacters);
		}
		public static string FormatLargeNumber(decimal number, int maxCharacters){
			// Removes the part of the number that is after the last visible digit. This is needed because Single.ToString rounds those values.
			number -= number%InverseBase;
			string untruncated = number.ToString("0.0", CultureInfo.InvariantCulture);
			int characterAmount = untruncated.Length;
			if (characterAmount <= maxCharacters){
				return untruncated;
			}
			((char character, long value), int emptyCharacterCount) = GetSuffix(characterAmount, maxCharacters);
			StringBuilder format = new("0.");
			// Display as many digits after the decimal as can fit.
			decimal smallestSigFig = 1;
			for (int i = 0; i < emptyCharacterCount; i++){
				format.Append('0');
				smallestSigFig *= InverseBase;
			}
			decimal numberSuffixScaled = number/value;
			// See first comment in this method.
			numberSuffixScaled -= numberSuffixScaled%smallestSigFig;
			return $"{numberSuffixScaled.ToString(format.ToString(), CultureInfo.InvariantCulture)}{character}";
		}
		public static string FormatLargeNumber(long number, int maxCharacters){
			string untruncated = number.ToString();
			int characterAmount = untruncated.Length;
			if (characterAmount <= maxCharacters){
				return untruncated;
			}
			((char character, long value), int emptyCharacterCount) = GetSuffix(characterAmount, maxCharacters);
			StringBuilder formattedNumber = new((number/value).ToString());
			// Sometimes it's possible to squeeze in one extra sig-fig.
			if (emptyCharacterCount == SuffixDigitStep){
				formattedNumber.Append($".{number*Base/value % Base}");
			}
			formattedNumber.Append(character);
			return formattedNumber.ToString();
		}
		private static ((char, long), int) GetSuffix(int characterAmount, int maxCharacters){
			foreach ((char, long) suffix in Suffixes){
				characterAmount -= SuffixDigitStep;
				if (characterAmount < maxCharacters){
					return (suffix, maxCharacters-characterAmount);
				}
			}
			// In case no suffix works just use the largest one.
			return (Suffixes[^1], 0);
		}
		
		public static string SignedPercent(float value){
			return $"{Signed(ToPercent(value))}%";
		}
		public static string Signed(int value){
			char sign = value < 0 ? '-' : '+';
			return $"{sign}{Mathf.Abs(value)}";
		}
		public static string Percent(float value){
			return $"{ToPercent(value)}%";
		}
		private static int ToPercent(float value){
			return Mathf.RoundToInt(value*Cent);
		}
	}
}