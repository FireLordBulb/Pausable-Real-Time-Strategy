using System;
using System.Linq;
using Simulation;
using UnityEngine;

namespace AI {
	public class AIController : MonoBehaviour {
		// TODO: Move constants to be serialized fields of a ScriptableObject when AIController gets added to the country prefab.
		private const float BasePeaceReluctance = -10;
		private const float AcceptancePerMilitaryStrengthDifference = 10;
		private const float MinMilitaryStrengthForAcceptanceMath = 10000;
		private const float AcceptancePerProvinceHeld = 2;
		private const float AcceptancePerDevelopmentHeld = 10;
		private const float MaxAcceptanceDecreaseFromResources = -20;
		private const float AcceptancePerGoldAvailable = -0.1f;
		private const float AcceptancePerManpowerAvailable = -0.01f;
		private const float AcceptancePerProvinceDemanded = -1;
		private const float AcceptancePerDevelopmentDemanded = -3;
		private const float UnoccupiedAcceptanceMultiplier = 5;
		private const float AcceptanceFromProvinceProportion = -10;
		private const float AcceptanceFromDevelopmentProportion = -30;
		private const float AcceptancePerGoldDemanded = -0.05f;
		private const float ForceAcceptance = 1000;
		
		public static int EvaluatePeaceOffer(PeaceTreaty treaty){
			// White peace logic
			if (treaty.IsWhitePeace){
				(Country loser, Country winner) = treaty.DidTreatyInitiatorWin ? (treaty.Loser, treaty.Winner) : (treaty.Winner, treaty.Loser);
				return CalculateDefeatAcceptance(treaty, loser, winner, CalculateReluctance(loser, winner));
			}
			// Losing logic
			if (treaty.DidTreatyInitiatorWin){
				return CalculateDefeatAcceptance(treaty, treaty.Loser, treaty.Winner, CalculateReluctance(treaty.Loser, treaty.Winner));
			}
			// Winning logic
			// If the AI would refuse the peace deal if the countries were switched then it must be favourable to the winner.
			return -CalculateDefeatAcceptance(treaty, treaty.Winner, treaty.Loser, -CalculateReluctance(treaty.Winner, treaty.Loser));
		}
		private static float CalculateReluctance(Country loser, Country winner){
			float acceptance = BasePeaceReluctance;
			
			float fromResources = loser.Gold*AcceptancePerGoldAvailable+loser.Manpower*AcceptancePerManpowerAvailable;
			fromResources = Math.Max(fromResources, MaxAcceptanceDecreaseFromResources);
			acceptance += fromResources;
			
			// TODO Factor in own & opponent other ongoing wars after war dec logic added.
			// TODO: Factor in navy when navy combat is added.
			
			float loserMilitaryStrength = GetMilitaryStrength(loser);
			float winnerMilitaryStrength = GetMilitaryStrength(winner);
			acceptance += (winnerMilitaryStrength/loserMilitaryStrength-1)*AcceptancePerMilitaryStrengthDifference; 

			float loserOccupationValue = GetOccupationValue(loser, winner);
			float winnerOccupationValue = GetOccupationValue(winner, loser);
			acceptance += winnerOccupationValue-loserOccupationValue;
			
			return acceptance;
		}
		private static int CalculateDefeatAcceptance(PeaceTreaty treaty, Country loser, Country winner, float reluctance){
			float acceptance = reluctance;
			if (treaty.IsWhitePeace){
				return (int)acceptance;
			}
			
			int demandedProvinceCount = 0;
			float demandedDevelopment = 0;
			foreach (Land land in treaty.AnnexedLands.Where(annexedLand => annexedLand.Owner == treaty.Loser)){
				demandedProvinceCount++;
				float development = 1+land.Terrain.DevelopmentModifier;
				demandedDevelopment += development;
				float provinceCost = AcceptancePerProvinceDemanded+development*AcceptancePerDevelopmentDemanded;
				if (land.Occupier != winner){
					provinceCost *= UnoccupiedAcceptanceMultiplier;
				}
				acceptance += provinceCost;
			}
			acceptance += AcceptanceFromProvinceProportion*demandedProvinceCount/loser.ProvinceCount;
			float totalLoserDevelopment = loser.Provinces.Sum(land => 1+land.Terrain.DevelopmentModifier);
			acceptance += AcceptanceFromDevelopmentProportion*demandedDevelopment/totalLoserDevelopment;
			
			acceptance += AcceptancePerGoldDemanded*treaty.GoldTransfer;
			
			// Always accept if fully occupied and all armies are defeated.
			if (loser.Provinces.All(land => land.Occupier == winner) && loser.Regiments.All(regiment => regiment.Province.Land.Owner != winner && regiment.Province.Land.Owner != loser)){
				acceptance += ForceAcceptance;
			// Otherwise always refuse full annexation.
			} else if (demandedProvinceCount == loser.ProvinceCount){
				acceptance -= ForceAcceptance;
			}
			
			return (int)acceptance;
		}
		private static float GetMilitaryStrength(Country country){
			return Math.Max(country.Regiments.Sum(regiment => regiment.CurrentManpower*(regiment.AttackPower+regiment.Toughness)), MinMilitaryStrengthForAcceptanceMath);
		}
		private static float GetOccupationValue(Country country, Country other){
			float value = 0;
			foreach (Land occupation in country.Occupations.Where(occupation => occupation.Owner == other)){
				float development = 1+occupation.Terrain.DevelopmentModifier;
				value += AcceptancePerProvinceHeld*development*AcceptancePerDevelopmentHeld;
			}
			return value;
		}
	}
}
