using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "SailToTarget", menuName = "ScriptableObjects/AI/Nodes/SailToTarget")]
	public class SailToTarget : MoveToTarget<Ship> {
		protected override MoveOrderResult OrderMove(Location<Ship> location){
			return Country.MoveFleetTo(Unit, location);
		}
	}
}
