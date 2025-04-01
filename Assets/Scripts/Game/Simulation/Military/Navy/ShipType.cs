using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "ShipType", menuName = "ScriptableObjects/Military/ShipType")]
	public class ShipType : UnitType<Navy> {
		public override bool CanBeBuiltBy(Country owner){
			throw new System.NotImplementedException();
		}
	}
}
