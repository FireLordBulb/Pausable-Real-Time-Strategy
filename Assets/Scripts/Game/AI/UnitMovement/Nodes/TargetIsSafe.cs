using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "TargetIsSafe", menuName = "ScriptableObjects/AI/Nodes/TargetIsSafe")]
	public class TargetIsSafe : BehaviourTree.Nodes.DecoratorNode {
		protected MilitaryUnitBrain<Regiment> Brain;
		private Province targetProvince;
		
		protected override void OnStart(){
			Brain = (MilitaryUnitBrain<Regiment>)Tree.TargetBrain;
			CurrentState = State.Running;
			targetProvince = Tree.Blackboard.GetValue<Province>(Brain.Target, null);
		}
		protected override State OnUpdate(){
			if (Brain.Controller.ShouldAvoidArmyAt(targetProvince, Brain.Unit)){
				// Cancel the movement if a big scary army moved into the path.
				Brain.Controller.Country.MoveRegimentTo(Brain.Unit, Brain.Unit.Province);
				CurrentState = State.Failure;
			} else {
				CurrentState = base.OnUpdate();
			}
			return CurrentState;
		}
	}
}
