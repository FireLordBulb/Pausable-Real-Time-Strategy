using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "StayInBattle", menuName = "ScriptableObjects/AI/Nodes/StayInBattle")]
	public class StayInBattle : MilitaryUnitNode<Regiment> {
		protected override State OnUpdate(){
			if (!Unit.Location.IsBattleOngoing && !Unit.IsRetreating){
				CurrentState = State.Failure;
			}
			return CurrentState;
		}
	}
}
