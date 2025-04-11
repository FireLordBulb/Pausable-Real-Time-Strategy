using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "RegimentType", menuName = "ScriptableObjects/Military/RegimentType")]
	public class RegimentType : UnitType<Regiment> {
		[SerializeField] private int manpower;
		[Header("Combat values")]
		[SerializeField] private float attackPower;
		[SerializeField] private float toughness;
		[SerializeField] private float killRate;
		
		public override bool CanBeBuiltBy(Country owner){
			return manpower <= owner.Manpower && goldCost <= owner.Gold;
		}
		public override void ApplyValuesTo(Regiment unit){
			unit.Init(attackPower, toughness, killRate, manpower);
		}
		public override void ConsumeBuildCostFrom(Country owner){
			owner.ChangeResources(-goldCost, -manpower, 0);
		}
		
		public override string GetCostAsString(){
			return $"{goldCost} Gold + {manpower} Manpower";
		}
		public override string CreatedVerb => "recruited";
	}
}
