using BehaviourTree;
using Simulation;
using Simulation.Military;

namespace AI.Nodes {
	public abstract class UnitDecorator<TUnit> : BehaviourTree.Nodes.DecoratorNode where TUnit : Unit<TUnit> {
		protected MilitaryUnitBrain<TUnit> Brain;

		protected Country Country => Controller.Country;
		protected AIController Controller => Brain.Controller;
		protected TUnit Unit => Brain.Unit;
		protected Blackboard Blackboard => Tree.Blackboard; 
		
		protected override void OnStart(){
			Brain = (MilitaryUnitBrain<TUnit>)Tree.TargetBrain;
			CurrentState = State.Running;
		}
		protected override State OnUpdate(){
			if (Predicate()){
				CurrentState = base.OnUpdate();
			} else {
				OnFailure();
				CurrentState = State.Failure;
			}
			return CurrentState;
		}
		protected abstract bool Predicate();
		protected virtual void OnFailure(){}
		
	}
}
