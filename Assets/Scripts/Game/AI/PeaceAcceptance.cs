using System;
using System.Linq;
using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "PeaceAcceptance", menuName = "ScriptableObjects/AI/PeaceAcceptance")]
	public class PeaceAcceptance : ScriptableObject {
		[Header("Reluctance")]
		[SerializeField] private float baseReluctance;
		[Header("War Situation")]
		[SerializeField] private float militaryStrength;
		[SerializeField] private float militaryStrengthMax;
		[SerializeField] private float minMilitaryStrengthValue;
		[SerializeField] private float navyWeight;
		[SerializeField] private float thirdPartyMultiplier;
		[SerializeField] private float provinceHeld;
		[SerializeField] private float developmentHeld;
		[SerializeField] private float allProvincesOccupied;
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
			bool isLoserDefeated  = IsFullyDefeated(treaty.Loser, treaty.Winner);
			bool isWinnerDefeated = IsFullyDefeated(treaty.Winner, treaty.Loser);
			
			// Losing logic
			if (treaty.DidTreatyInitiatorWin){
				acceptance = Reluctance(treaty.Loser, treaty.Winner);
				if (isLoserDefeated){
					acceptance += always;
				} else if (isWinnerDefeated){
					acceptance += never;
				} else {
					acceptance += FullOccupationAcceptance(treaty.Loser, treaty.Winner);
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
				} else {
					acceptance -= FullOccupationAcceptance(treaty.Winner, treaty.Loser);
				}
				acceptance -= WarGoalCost(treaty, winGoldDemandedMin);
			}
			// Assign back to the original value so that evaluating doesn't permanently modify anything in the treaty. 
			treaty.DidTreatyInitiatorWin = didInitiatorWin;
			return Mathf.RoundToInt(acceptance);
		}
		
		private float Reluctance(Country decider, Country other){
			float acceptance = baseReluctance;
			
			float deciderMilitaryStrength = GetSituationalMilitaryStrength(decider, other);
			float otherMilitaryStrength = GetSituationalMilitaryStrength(other, decider);
			acceptance += Mathf.Min((otherMilitaryStrength/deciderMilitaryStrength-1)*militaryStrength, militaryStrengthMax); 

			float deciderOccupationValue = GetOccupationValue(decider, other);
			float otherOccupationValue = GetOccupationValue(other, decider);
			acceptance += otherOccupationValue-deciderOccupationValue;
			
			return acceptance;
		}
		private float GetSituationalMilitaryStrength(Country country, Country secondParty){
			float strength = GetMilitaryStrength(country);
			strength -= GetThirdPartyMilitaryStrength(country, secondParty)*thirdPartyMultiplier;
			return Mathf.Max(strength, minMilitaryStrengthValue);
		}
		private float GetThirdPartyMilitaryStrength(Country country, Country secondParty){
			AIController borderingAI = country.GetComponent<AIController>();
			return borderingAI.WarEnemies.Sum(enemy => enemy.Country != secondParty ? GetMilitaryStrength(enemy.Country) : 0);
		}
		private float GetMilitaryStrength(Country country){
			return country.Regiments.Sum(AIController.RegimentStrength)+navyWeight*country.Ships.Sum(AIController.ShipStrength);
		}
		private float GetOccupationValue(Country occupier, Country target){
			float value = 0;
			foreach (Land occupation in occupier.Occupations.Where(occupation => occupation.Owner == target)){
				float development = occupation.Development;
				value += provinceHeld/occupier.ProvinceCount+developmentHeld*development/target.TotalDevelopment;
			}
			return value;
		}

		private float FullOccupationAcceptance(Country loser, Country winner){
			if (IsFullyOccupied(loser, winner)){
				return allProvincesOccupied;
			}
			if (IsFullyOccupied(winner, loser)){
				return -allProvincesOccupied;
			}
			return 0;
		}
		private static bool IsFullyDefeated(Country decider, Country other){
			return decider.Provinces.All(land => land.Occupier == other) &&
			       other.Provinces.All(land => land.Occupier != decider) &&
			       AreRegimentsFullyDefeated(decider, other);
		}
		private static bool IsFullyOccupied(Country decider, Country other){
			return decider.Provinces.All(land => land.IsOccupied) &&
			       other.Provinces.All(land => land.Occupier != decider) &&
			       AreRegimentsFullyDefeated(decider, other);
		}
		private static bool AreRegimentsFullyDefeated(Country decider, Country other){
			return decider.Regiments.All(regiment => regiment.Location is not Simulation.Military.LandLocation landLocation || landLocation.Land.Owner != other && landLocation.Land.Owner != decider);
		}
		private static bool IsFullAnnexationDemanded(PeaceTreaty treaty){
			return treaty.Loser.ProvinceCount == treaty.AnnexedLands.Count(land => land.Owner == treaty.Loser);
		}
		
		private float WarGoalCost(PeaceTreaty treaty, float maxGoldAcceptanceDecrease){
			float acceptance = 0;
			
			float totalLoserDevelopment = treaty.Loser.TotalDevelopment;
			foreach (Land land in treaty.AnnexedLands.Where(annexedLand => annexedLand.Owner == treaty.Loser)){
				float development = land.Development;
				float provinceCost = provincesDemanded/treaty.Loser.ProvinceCount+development*developmentDemanded/totalLoserDevelopment;
				if (land.Occupier != treaty.Winner){
					provinceCost *= unoccupiedMultiplier;
				}
				acceptance += provinceCost;
			}
			
			acceptance += Math.Max(goldDemanded*treaty.GoldTransfer, maxGoldAcceptanceDecrease);
			
			return acceptance;
		}
	}
}