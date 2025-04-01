using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "RegimentType", menuName = "ScriptableObjects/Military/RegimentType")]
	public class RegimentType : UnitType<Army> {
		[SerializeField] private int manpower;
		public override bool CanBeBuiltBy(Country owner){
			return manpower <= owner.Manpower && goldCost <= owner.Gold;
		}
		public override void ConsumeBuildCostFrom(Country owner){
			owner.GainResources(-goldCost, -manpower, 0);
		}
	}
}
