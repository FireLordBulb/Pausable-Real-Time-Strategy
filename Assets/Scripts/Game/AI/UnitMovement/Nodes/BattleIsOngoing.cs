using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "BattleIsOngoing", menuName = "ScriptableObjects/AI/Nodes/BattleIsOngoing")]
	public class BattleIsOngoing : BehaviourTree.Nodes.DecoratorNode {
		protected MilitaryUnitBrain<Regiment> Brain;
		private Province targetProvince;
		
		protected override void OnStart(){
			Brain = (MilitaryUnitBrain<Regiment>)Tree.TargetBrain;
			CurrentState = State.Running;
			targetProvince = Tree.Blackboard.GetValue<Province>(Brain.Target, null);
		}
		protected override State OnUpdate(){
			if (!Brain.IsReinforceableBattleOngoing(targetProvince.Land.ArmyLocation)){
				Brain.Controller.Country.MoveRegimentTo(Brain.Unit, Brain.Unit.Province);
				CurrentState = State.Failure;
			} else {
				CurrentState = base.OnUpdate();
			}
			return CurrentState;
		}
	}
}
