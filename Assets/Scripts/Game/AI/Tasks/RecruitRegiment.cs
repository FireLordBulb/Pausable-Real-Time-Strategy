using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "RecruitRegiment", menuName = "ScriptableObjects/AI/Tasks/RecruitRegiment")]
	public class RecruitRegiment : Task {
		[SerializeField] private RegimentType regimentType;
		private Land recruitmentProvince;
		
		protected override int CurrentPriority(){
			if (!RegimentProvinceIsValid()){
				recruitmentProvince = GetRecruitmentProvince();
			}
			return CanBePerformed() ? +1 : -1;
		}
		public override bool CanBePerformed(){
			return recruitmentProvince != null && regimentType.CanBeBuiltBy(Controller.Country);
		}
		public override void Perform(){
			if (recruitmentProvince == null){
				return;
			}
			Controller.Country.TryStartRecruitingRegiment(regimentType, recruitmentProvince.Province);
		}

		private bool RegimentProvinceIsValid(){
			return recruitmentProvince != null &&
			       recruitmentProvince.Owner == Controller.Country &&
			       IsSafe(recruitmentProvince);
		}
		private Land GetRecruitmentProvince(){
			return Controller.Country.Provinces.FirstOrDefault(IsSafe);
		}
		private bool IsSafe(Land land){
			return !land.IsOccupied &&
			       land.ArmyLocation.Units.All(regiment => regiment.Owner == Controller.Country);
		}
	}
}