using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "NavyType", menuName = "ScriptableObjects/Military/NavyType")]
	public class NavyType : UnitType<Navy> {
		public override bool CanBeBuiltBy(Country owner){
			throw new System.NotImplementedException();
		}
	}
}
