using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "BattleIsOngoing", menuName = "ScriptableObjects/AI/Nodes/BattleIsOngoing")]
	public class BattleIsOngoing : TargetDecorator {
		protected override bool IsTargetValid(Location<Regiment> targetLocation){
			return Brain.IsReinforceableBattleOngoing(targetLocation);
		}
	}
}
