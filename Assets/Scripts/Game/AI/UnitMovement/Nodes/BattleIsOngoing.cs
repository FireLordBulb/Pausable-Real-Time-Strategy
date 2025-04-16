using Simulation;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "BattleIsOngoing", menuName = "ScriptableObjects/AI/Nodes/BattleIsOngoing")]
	public class BattleIsOngoing : TargetDecorator {
		protected override bool IsTargetValid(Province targetProvince){
			return Brain.IsReinforceableBattleOngoing(targetProvince.Land.ArmyLocation);
		}
	}
}
