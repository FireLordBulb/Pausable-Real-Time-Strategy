using System;
using System.Collections.Generic;
using System.Linq;
using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "PeaceAcceptance", menuName = "ScriptableObjects/AI/PeaceAcceptance")]
	public class PeaceAcceptance : ScriptableObject {
		[Header("Reluctance")]
		[SerializeField] private int baseReluctance;
		[Header("War Situation")]
		[SerializeField] private float militaryStrength;
		[SerializeField] private float militaryStrengthMax;
		[SerializeField] private float minMilitaryStrengthValue;
		[SerializeField] private float navyWeight;
		[SerializeField] private float thirdPartyMultiplier;
		[SerializeField] private float provinceHeld;
		[SerializeField] private float developmentHeld;
		[SerializeField] private int allProvincesOccupied;
		[Header("Harshness of Treaty Demands")]
		[SerializeField] private float provincesDemanded;
		[SerializeField] private float developmentDemanded;
		[SerializeField] private float unoccupiedMultiplier;
		[SerializeField] private float goldDemanded;
		[SerializeField] private float winGoldDemandedMin;
		[SerializeField] private float loseGoldDemandedMin;
		[Space]
		[SerializeField] private int always;
		[SerializeField] private int never;

		private int acceptance;
		private List<(int, string)> reasons;
		public (int, List<(int, string)>) EvaluatePeaceOffer(PeaceTreaty treaty){
			acceptance = 0;
			reasons = new List<(int, string)>();
			bool didInitiatorWin = treaty.DidTreatyInitiatorWin;
			// Count white peaces as wins (besides war goals being ignored, off course).
			if (treaty.IsWhitePeace){
				treaty.DidTreatyInitiatorWin = true;
			}
			bool isLoserDefeated  = IsFullyDefeated(treaty.Loser, treaty.Winner);
			bool isWinnerDefeated = IsFullyDefeated(treaty.Winner, treaty.Loser);
			
			// Losing logic
			if (treaty.DidTreatyInitiatorWin){
				AddReluctance(treaty.Loser, treaty.Winner);
				if (isLoserDefeated){
					AddReason(always, "Country Is Fully Defeated");
				} else if (isWinnerDefeated){
					AddReason(never, "Player Is Fully Defeated");
				} else {
					AddFullOccupationAcceptance(treaty.Loser, treaty.Winner);
				}
				if (!treaty.IsWhitePeace){
					// If not fully defeated, always refuse full annexation.
					if (!isLoserDefeated && IsFullAnnexationDemanded(treaty)){
						AddReason(never, "Demanding Full Annexation");
					}
					AddWarGoalCost(treaty, loseGoldDemandedMin, +1);
				}
			} else { // Winning logic
				AddReluctance(treaty.Winner, treaty.Loser);
				if (IsFullAnnexationDemanded(treaty)){
					AddReason(always, "Total Victory");
				} else if (isWinnerDefeated){
					AddReason(always, "Country Is Fully Defeated");
				} else if (isLoserDefeated){
					AddReason(never, "Player Is Fully Defeated");
				} else {
					AddFullOccupationAcceptance(treaty.Winner, treaty.Loser);
				}
				AddWarGoalCost(treaty, winGoldDemandedMin, -1);
			}
			// Assign back to the original value so that evaluating doesn't permanently modify anything in the Treaty. 
			treaty.DidTreatyInitiatorWin = didInitiatorWin;
			return (acceptance, reasons);
		}
		
		private void AddReluctance(Country decider, Country other){
			AddReason(baseReluctance, "Base Reluctance");
			
			float deciderMilitaryStrength = GetSituationalMilitaryStrength(decider, other);
			float otherMilitaryStrength = GetSituationalMilitaryStrength(other, decider);
			AddReason(Mathf.Min((otherMilitaryStrength/deciderMilitaryStrength-1)*militaryStrength, militaryStrengthMax), "Relative Military Strength"); 

			float deciderOccupationValue = GetOccupationValue(decider, other);
			float otherOccupationValue = GetOccupationValue(other, decider);
			AddReason(otherOccupationValue-deciderOccupationValue, "Relative Occupation");
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
		
		private void AddFullOccupationAcceptance(Country loser, Country winner){
			if (IsFullyOccupied(loser, winner)){
				AddReason(allProvincesOccupied, "Country Is Fully Occupied");
			} else if (IsFullyOccupied(winner, loser)){
				AddReason(-allProvincesOccupied, "Player Is Fully Occupied");
			}
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
		
		private void AddWarGoalCost(PeaceTreaty treaty, float maxGoldAcceptanceDecrease, int sign){
			float acceptanceFromLand = 0;
			float totalLoserDevelopment = treaty.Loser.TotalDevelopment;
			foreach (Land land in treaty.AnnexedLands.Where(annexedLand => annexedLand.Owner == treaty.Loser)){
				float development = land.Development;
				float provinceCost = provincesDemanded/treaty.Loser.ProvinceCount+development*developmentDemanded/totalLoserDevelopment;
				if (land.Occupier != treaty.Winner){
					provinceCost *= unoccupiedMultiplier;
				}
				acceptanceFromLand += provinceCost;
			}
			AddReason(acceptanceFromLand*sign, "Proportion of Land demanded");
			AddReason(Math.Max(goldDemanded*treaty.GoldTransfer, maxGoldAcceptanceDecrease)*sign, "Size of Gold Transfer");
		}

		private void AddReason(float value, string reason){
			AddReason(Mathf.RoundToInt(value), reason);
		}
		private void AddReason(int value, string reason){
			if (value == 0){
				return;
			}
			acceptance += value;
			reasons.Add((value, reason));
		}
	}
}