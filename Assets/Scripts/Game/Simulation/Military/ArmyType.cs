using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "ArmyType", menuName = "ScriptableObjects/Military/ArmyType")]
	public class ArmyType : UnitType<Army> {
		public override bool CanBeBuiltBy(Country owner){
			throw new System.NotImplementedException();
		}
	}
}
