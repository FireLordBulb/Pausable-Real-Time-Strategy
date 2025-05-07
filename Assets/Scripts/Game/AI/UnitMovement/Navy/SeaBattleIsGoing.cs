using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "SeaBattleIsGoing", menuName = "ScriptableObjects/AI/Nodes/SeaBattleIsGoing")]
	public class SeaBattleIsGoing : SailTargetDecorator {
		protected override bool IsTargetValid(Location<Ship> targetLocation){
			return Brain.IsReinforceableBattleOngoing(targetLocation);
		}
	}
}
