using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "RecruitRegiment", menuName = "ScriptableObjects/AI/Tasks/RecruitRegiment")]
	public class RecruitRegiment : Task {
		[SerializeField] private int enoughRegimentsPriority;
		[SerializeField] private int maxRegimentsPriority;
		[Space]
		[SerializeField] private RegimentType regimentType;
		[SerializeField] private float enoughRegimentsPerProvince;
		[SerializeField] private float maxRegimentsPerProvince;
	
		private Land recruitmentProvince;
		
		protected override int CurrentPriority(){
			if (Country.GoldIncome < regimentType.MaintenanceCost || Country.ProvinceCount*maxRegimentsPerProvince < Country.Regiments.Count){
				return maxRegimentsPriority;
			}
			if (Country.ProvinceCount*enoughRegimentsPerProvince < Country.Regiments.Count){
				return enoughRegimentsPriority;
			}
			return defaultPriority;
		}
		internal override bool CanBePerformed(){
			// Refuse to build any regiments if at max per province.
			if (Priority == maxRegimentsPriority){
				return false;
			}
			if (!RegimentProvinceIsValid()){
				recruitmentProvince = GetRecruitmentProvince();
			}
			return recruitmentProvince != null && regimentType.CanBeBuiltBy(Country);
		}
		internal override void Perform(){
			if (recruitmentProvince == null){
				return;
			}
			Country.TryStartRecruitingRegiment(regimentType, recruitmentProvince.Province);
		}

		private bool RegimentProvinceIsValid(){
			return recruitmentProvince != null &&
			       recruitmentProvince.Owner == Country &&
			       IsSafe(recruitmentProvince);
		}
		private Land GetRecruitmentProvince(){
			return Country.Provinces.FirstOrDefault(IsSafe);
		}
		private bool IsSafe(Land land){
			return !land.IsOccupied &&
			       land.ArmyLocation.Units.All(regiment => regiment.Owner == Controller.Country);
		}
	}
}