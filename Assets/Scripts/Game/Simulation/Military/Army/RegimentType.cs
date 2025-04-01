using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "RegimentType", menuName = "ScriptableObjects/Military/RegimentType")]
	public class RegimentType : UnitType<Army> {
		public override bool CanBeBuiltBy(Country owner){
			throw new System.NotImplementedException();
		}
	}
}
