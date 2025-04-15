using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "KeepBesieging", menuName = "ScriptableObjects/AI/Nodes/KeepBesieging")]
	public class KeepBesieging : MilitaryUnitNode<Regiment> {
		private Province target;
		protected override State OnUpdate(){
			if (Brain.Unit.Location is not LandLocation{SiegeIsOngoing: true}){
				CurrentState = State.Failure;
			}
			return CurrentState;
		}
	}
}
