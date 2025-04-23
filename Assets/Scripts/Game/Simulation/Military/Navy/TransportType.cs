using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "TransportType", menuName = "ScriptableObjects/Military/TransportType")]
	public class TransportType : ShipType {
		[Header("Army Transportation")]
		[SerializeField] private int manpowerCapacity;
		public override void ApplyValuesTo(Ship unit){
			if (unit is Transport transport){
				ApplyValuesTo(transport);
			} else {
				base.ApplyValuesTo(unit);
			}
		}
		public void ApplyValuesTo(Transport transport){
			transport.Init(attackPower, hull, size, goldCost, sailors, manpowerCapacity);
		}
	}
}
