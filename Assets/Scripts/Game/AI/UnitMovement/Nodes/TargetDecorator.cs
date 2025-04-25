using Simulation.Military;

namespace AI.Nodes {
	public abstract class TargetDecorator : BehaviourTree.Nodes.DecoratorNode {
		protected MilitaryUnitBrain<Regiment> Brain;
		private Location<Regiment> targetLocation;
		
		protected override void OnStart(){
			Brain = (MilitaryUnitBrain<Regiment>)Tree.TargetBrain;
			CurrentState = State.Running;
			targetLocation = Tree.Blackboard.GetValue<Location<Regiment>>(Brain.Target, null);
		}
		protected override State OnUpdate(){
			if (targetLocation != null && IsTargetValid(targetLocation)){
				CurrentState = base.OnUpdate();
			} else {
				Brain.Controller.Country.MoveRegimentTo(Brain.Unit, Brain.Unit.Location);
				CurrentState = State.Failure;
			}
			return CurrentState;
		}
		protected abstract bool IsTargetValid(Location<Regiment> targetLocation);
	}
}
