using System;
using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "PeaceAcceptance", menuName = "ScriptableObjects/AI/PeaceAcceptance")]
	public class PeaceAcceptance : ScriptableObject {
		[Header("Reluctance")]
		[SerializeField] private float baseReluctance;
		[SerializeField] private float resourcesMin;
		[SerializeField] private float goldAvailable;
		[SerializeField] private float manpowerAvailable;
		[Header("War Situation")]
		[SerializeField] private float militaryStrength;
		[SerializeField] private float militaryStrengthMax;
		[SerializeField] private float minMilitaryStrengthValue;
		[SerializeField] private float provinceHeld;
		[SerializeField] private float developmentHeld;
		[Header("Harshness of Treaty Demands")]
		[SerializeField] private float provincesDemanded;
		[SerializeField] private float developmentDemanded;
		[SerializeField] private float unoccupiedMultiplier;
		[SerializeField] private float goldDemanded;
		[SerializeField] private float winGoldDemandedMin;
		[SerializeField] private float loseGoldDemandedMin;
		[Space]
		[SerializeField] private float always;
		[SerializeField] private float never;
		
		public int EvaluatePeaceOffer(PeaceTreaty treaty){
			float acceptance;
			bool didInitiatorWin = treaty.DidTreatyInitiatorWin;
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
					acceptance += always;
				} else if (isWinnerDefeated){
					acceptance += never;
				}
				if (!treaty.IsWhitePeace){
					// If not fully defeated, always refuse full annexation.
					if (!isLoserDefeated && IsFullAnnexationDemanded(treaty)){
						acceptance += never;
					}
					acceptance += WarGoalCost(treaty, loseGoldDemandedMin);
				}
			} else { // Winning logic
				acceptance = Reluctance(treaty.Winner, treaty.Loser);
				if (IsFullAnnexationDemanded(treaty) || isWinnerDefeated){
					acceptance += always;
				} else if (isLoserDefeated){
					acceptance += never;
				}
				acceptance -= WarGoalCost(treaty, winGoldDemandedMin);
			}
			// Assign back to the original value so that evaluating doesn't permanently modify anything in the treaty. 
			treaty.DidTreatyInitiatorWin = didInitiatorWin;
			return Mathf.RoundToInt(acceptance);
		}
		
		private float Reluctance(Country decider, Country other){
			float acceptance = baseReluctance;
			
			float fromResources = decider.Gold*goldAvailable+decider.Manpower*manpowerAvailable;
			fromResources = Math.Max(fromResources, resourcesMin);
			acceptance += fromResources;
			
			// TODO Factor in own & opponent other ongoing wars after war dec logic added.
			// TODO: Factor in navy when navy combat is added.
			
			float deciderMilitaryStrength = GetMilitaryStrength(decider);
			float otherMilitaryStrength = GetMilitaryStrength(other);
			acceptance += Mathf.Min((otherMilitaryStrength/deciderMilitaryStrength-1)*militaryStrength, militaryStrengthMax); 

			float deciderOccupationValue = GetOccupationValue(decider, other);
			float otherOccupationValue = GetOccupationValue(other, decider);
			acceptance += otherOccupationValue-deciderOccupationValue;
			
			return acceptance;
		}
		private float GetMilitaryStrength(Country country){
			return Math.Max(country.Regiments.Sum(RegimentStrength), minMilitaryStrengthValue);
		}
		private static float RegimentStrength(Regiment regiment){
			return (regiment.CurrentManpower+regiment.DemoralizedManpower)*(regiment.AttackPower+regiment.Toughness);
		}
		private float GetOccupationValue(Country occupier, Country target){
			float value = 0;
			float totalDevelopment = TotalDevelopment(target);
			foreach (Land occupation in occupier.Occupations.Where(occupation => occupation.Owner == target)){
				float development = 1+occupation.Terrain.DevelopmentModifier;
				value += provinceHeld/occupier.ProvinceCount*development*developmentHeld/totalDevelopment;
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
		
		private float WarGoalCost(PeaceTreaty treaty, float maxGoldAcceptanceDecrease){
			float acceptance = 0;
			
			float totalLoserDevelopment = TotalDevelopment(treaty.Loser);
			foreach (Land land in treaty.AnnexedLands.Where(annexedLand => annexedLand.Owner == treaty.Loser)){
				float development = 1+land.Terrain.DevelopmentModifier;
				float provinceCost = provincesDemanded/treaty.Loser.ProvinceCount+development*developmentDemanded/totalLoserDevelopment;
				if (land.Occupier != treaty.Winner){
					provinceCost *= unoccupiedMultiplier;
				}
				acceptance += provinceCost;
			}
			
			acceptance += Math.Max(goldDemanded*treaty.GoldTransfer, maxGoldAcceptanceDecrease);
			
			return acceptance;
		}
		
		// TODO: Keep cached in Country after proper development values are added.
		private static float TotalDevelopment(Country country) => country.Provinces.Sum(land => 1+land.Terrain.DevelopmentModifier);
	}
}