using Simulation;
using Simulation.Military;

namespace AI.Nodes {
	public abstract class TargetDecorator : BehaviourTree.Nodes.DecoratorNode {
		protected MilitaryUnitBrain<Regiment> Brain;
		private Province targetProvince;
		
		protected override void OnStart(){
			Brain = (MilitaryUnitBrain<Regiment>)Tree.TargetBrain;
			CurrentState = State.Running;
			targetProvince = Tree.Blackboard.GetValue<Province>(Brain.Target, null);
		}
		protected override State OnUpdate(){
			if (IsTargetValid(targetProvince)){
				CurrentState = base.OnUpdate();
			} else {
				Brain.Controller.Country.MoveRegimentTo(Brain.Unit, Brain.Unit.Location);
				CurrentState = State.Failure;
			}
			return CurrentState;
		}
		protected abstract bool IsTargetValid(Province targetProvince);
	}
}
