namespace AI.Nodes {
	public abstract class MilitaryUnitNode<TUnit> : BehaviourTree.Nodes.ActionNode where TUnit : Simulation.Military.Unit<TUnit> {
		protected MilitaryUnitBrain<TUnit> Brain;
		
		protected override void OnStart(){
			Brain = (MilitaryUnitBrain<TUnit>)Tree.TargetBrain;
			CurrentState = State.Running;
		}
		protected override void OnStop(){}
		protected override State OnUpdate(){
			return CurrentState;	
		}
	}
}