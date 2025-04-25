using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "MarchToTarget", menuName = "ScriptableObjects/AI/Nodes/MarchToTarget")]
	public class MarchToTarget : MoveToTarget<Regiment> {
		protected override MoveOrderResult OrderMove(Location<Regiment> location){
			return Country.MoveRegimentTo(Unit, location);
		}
	}
}
