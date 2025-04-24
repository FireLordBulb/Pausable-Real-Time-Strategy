using UnityEngine;

namespace Simulation.Military {
	[CreateAssetMenu(fileName = "TransportType", menuName = "ScriptableObjects/Military/TransportType")]
	public class TransportType : ShipType {
		[Header("Army Transportation")]
		[SerializeField] private int manpowerCapacity;
		public override void ApplyValuesTo(Ship unit){
			base.ApplyValuesTo(unit);
			((Transport)unit).Init(manpowerCapacity);
		}
	}
}
