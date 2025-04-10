using System;
using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	public class AIController : MonoBehaviour {
		// TODO: Move constants to be serialized fields of a ScriptableObject when AIController gets added to the country prefab.
		// Also move acceptance logic into that SO (it can get the AIController as a parameter if needed)
		private const float BasePeaceReluctance = -10;
		private const float MaxAcceptanceDecreaseFromResources = -20;
		private const float AcceptancePerGoldAvailable = -0.02f;
		private const float AcceptancePerManpowerAvailable = -0.002f;
		
		private const float AcceptanceFromMilitaryStrength = 10;
		private const float MaxAcceptanceFromMilitaryStrength = 30;
		private const float MinMilitaryStrengthForAcceptanceMath = 5000;
		private const float AcceptanceFromProvinceHeld = 10;
		private const float AcceptanceFromDevelopmentHeld = 30;
		
		private const float AcceptanceFromProvincesDemanded = -5;
		private const float AcceptanceFromDevelopmentDemanded = -40;
		private const float UnoccupiedAcceptanceMultiplier = 3;
		private const float AcceptancePerGoldDemanded = -0.05f;
		private const float MaxAcceptanceDecreaseFromGoldDemanded = -50;
		
		private const float AlwaysAccept = 1000;
		private const float NeverAccept = -1000;
		
		public static int EvaluatePeaceOffer(PeaceTreaty treaty){
			float acceptance;
			// Count white peaces as wins (besides war goals being ignored, off course).
			if (treaty.IsWhitePeace){
				treaty.DidTreatyInitiatorWin = true;
			}
			bool isLoserDefeated = IsFullyDefeated(treaty.Loser, treaty.Winner);
			bool isWinnerDefeated = IsFullyDefeated(treaty.Winner, treaty.Loser);
			// Losing logic
			if (treaty.DidTreatyInitiatorWin){
				acceptance = Reluctance(treaty.Loser, treaty.Winner);
				if (isLoserDefeated){
					acceptance += AlwaysAccept;
				} else if (isWinnerDefeated){
					acceptance += NeverAccept;
				}
				if (!treaty.IsWhitePeace){
					// If not fully defeated, always refuse full annexation.
					if (!isLoserDefeated && IsFullAnnexationDemanded(treaty)){
						acceptance += NeverAccept;
					}
					acceptance += WarGoalCost(treaty, NeverAccept/2);
				}
			} else { // Winning logic
				acceptance = Reluctance(treaty.Winner, treaty.Loser);
				if (IsFullAnnexationDemanded(treaty) || isWinnerDefeated){
					acceptance += AlwaysAccept;
				} else if (isLoserDefeated){
					acceptance += NeverAccept;
				}
				acceptance -= WarGoalCost(treaty, MaxAcceptanceDecreaseFromGoldDemanded);
			}
			return Mathf.RoundToInt(acceptance);
		}
		
		private static float Reluctance(Country decider, Country other){
			float acceptance = BasePeaceReluctance;
			
			float fromResources = decider.Gold*AcceptancePerGoldAvailable+decider.Manpower*AcceptancePerManpowerAvailable;
			fromResources = Math.Max(fromResources, MaxAcceptanceDecreaseFromResources);
			acceptance += fromResources;
			
			// TODO Factor in own & opponent other ongoing wars after war dec logic added.
			// TODO: Factor in navy when navy combat is added.
			
			float deciderMilitaryStrength = GetMilitaryStrength(decider);
			float otherMilitaryStrength = GetMilitaryStrength(other);
			acceptance += Mathf.Min((otherMilitaryStrength/deciderMilitaryStrength-1)*AcceptanceFromMilitaryStrength, MaxAcceptanceFromMilitaryStrength); 

			float deciderOccupationValue = GetOccupationValue(decider, other);
			float otherOccupationValue = GetOccupationValue(other, decider);
			acceptance += otherOccupationValue-deciderOccupationValue;
			
			return acceptance;
		}
		private static float GetMilitaryStrength(Country country){
			return Math.Max(country.Regiments.Sum(RegimentStrength), MinMilitaryStrengthForAcceptanceMath);
		}
		private static float RegimentStrength(Regiment regiment){
			return (regiment.CurrentManpower+regiment.DemoralizedManpower)*(regiment.AttackPower+regiment.Toughness);
		}
		private static float GetOccupationValue(Country occupier, Country target){
			float value = 0;
			float totalDevelopment = TotalDevelopment(target);
			foreach (Land occupation in occupier.Occupations.Where(occupation => occupation.Owner == target)){
				float development = 1+occupation.Terrain.DevelopmentModifier;
				value += AcceptanceFromProvinceHeld/occupier.ProvinceCount*development*AcceptanceFromDevelopmentHeld/totalDevelopment;
			}
			return value;
		}
		
		private static bool IsFullyDefeated(Country decider, Country other){
			return decider.Provinces.All(land => land.Occupier == other) &&
			       other.Provinces.All(land => land.Occupier != decider) &&
			       decider.Regiments.All(regiment => regiment.Province.Land.Owner != other && regiment.Province.Land.Owner != decider);
		}
		private static bool IsFullAnnexationDemanded(PeaceTreaty treaty){
			return treaty.Loser.ProvinceCount == treaty.AnnexedLands.Count(land => land.Owner == treaty.Loser);
		}
		
		private static float WarGoalCost(PeaceTreaty treaty, float maxGoldAcceptanceDecrease){
			float acceptance = 0;
			
			float totalLoserDevelopment = TotalDevelopment(treaty.Loser);
			foreach (Land land in treaty.AnnexedLands.Where(annexedLand => annexedLand.Owner == treaty.Loser)){
				float development = 1+land.Terrain.DevelopmentModifier;
				float provinceCost = AcceptanceFromProvincesDemanded/treaty.Loser.ProvinceCount+development*AcceptanceFromDevelopmentDemanded/totalLoserDevelopment;
				if (land.Occupier != treaty.Winner){
					provinceCost *= UnoccupiedAcceptanceMultiplier;
				}
				acceptance += provinceCost;
			}
			
			acceptance += Math.Max(AcceptancePerGoldDemanded*treaty.GoldTransfer, maxGoldAcceptanceDecrease);
			
			return acceptance;
		}
		
		// TODO: Keep cached in Country after proper development values are added.
		private static float TotalDevelopment(Country country) => country.Provinces.Sum(land => 1+land.Terrain.DevelopmentModifier);
	}
}
