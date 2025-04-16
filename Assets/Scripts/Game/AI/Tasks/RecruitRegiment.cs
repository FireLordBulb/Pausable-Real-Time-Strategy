using System.Linq;
using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "RecruitRegiment", menuName = "ScriptableObjects/AI/Tasks/RecruitRegiment")]
	public class RecruitRegiment : Task {
		[SerializeField] private Simulation.Military.RegimentType regimentType;
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
			return recruitmentProvince != null && !recruitmentProvince.IsOccupied && recruitmentProvince.Owner == Controller.Country;
		}
		private Land GetRecruitmentProvince(){
			return Controller.Country.Provinces.FirstOrDefault(land => !land.IsOccupied);
		}
	}
}