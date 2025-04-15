using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "StayInBattle", menuName = "ScriptableObjects/AI/Nodes/StayInBattle")]
	public class StayInBattle : MilitaryUnitNode<Regiment> {
		protected override State OnUpdate(){
			if (!Brain.Unit.Location.IsBattleOngoing || !Brain.Unit.IsRetreating){
				CurrentState = State.Failure;
			}
			return CurrentState;
		}
	}
}
