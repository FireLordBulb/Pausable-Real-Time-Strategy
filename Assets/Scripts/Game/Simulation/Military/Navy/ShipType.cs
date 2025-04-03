using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "ShipType", menuName = "ScriptableObjects/Military/ShipType")]
	public class ShipType : UnitType<Navy> {
		[SerializeField] private int sailors;
		public override bool CanBeBuiltBy(Country owner){
			return sailors <= owner.Manpower && goldCost <= owner.Gold;
		}
		public override void ConsumeBuildCostFrom(Country owner){
			owner.GainResources(-goldCost, 0, -sailors);
		}
		public override string GetCostAsString(){
			return $"Gold: {goldCost} + Sailors : {sailors}";
		}
		public override string CreatedVerb => "constructed";
	}
}
