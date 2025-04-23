using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "TransportType", menuName = "ScriptableObjects/Military/TransportType")]
	public class TransportType : ShipType {
		[Header("Army Transportation")]
		[SerializeField] private int manpowerCapacity;
		
		public void ApplyValuesTo(Transport unit){
			Debug.Log("Overloads!!");
			unit.Init(attackPower, hull, size, goldCost, sailors, manpowerCapacity);
		}
	}
}
