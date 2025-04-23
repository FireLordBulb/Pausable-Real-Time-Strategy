using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "ShipType", menuName = "ScriptableObjects/Military/ShipType")]
	public class ShipType : UnitType<Ship> {
		[SerializeField] protected int sailors;
		[Header("Combat")]
		[SerializeField] protected float attackPower;
		[SerializeField] protected int hull;
		[SerializeField] protected int size;
		
		public override bool CanBeBuiltBy(Country owner){
			return sailors <= owner.Sailors && goldCost <= owner.Gold;
		}
		public override void ApplyValuesTo(Ship unit){
			unit.Init(attackPower, hull, size, goldCost, sailors);
		}
		public override void ConsumeBuildCostFrom(Country owner){
			owner.ChangeResources(-goldCost, 0, -sailors);
		}
		public override string GetCostAsString(){
			return $"Gold: {goldCost} + Sailors : {sailors}";
		}
		public override string CreatedVerb => "constructed";
	}
}
