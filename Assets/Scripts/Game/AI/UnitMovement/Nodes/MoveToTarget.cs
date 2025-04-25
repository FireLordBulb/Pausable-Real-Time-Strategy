using Simulation.Military;

namespace AI.Nodes {
	public abstract class MoveToTarget<TUnit> : MilitaryUnitNode<TUnit> where TUnit : Unit<TUnit> {
		private Location<TUnit> targetLocation;
		protected override void OnStart(){
			base.OnStart();
			targetLocation = Tree.Blackboard.GetValue<Location<TUnit>>(Brain.Target, null);
			MoveOrderResult result = targetLocation == null ? MoveOrderResult.InvalidTarget : OrderMove(targetLocation);
			CurrentState = result == MoveOrderResult.Success ? State.Running : State.Failure;
		}
		protected abstract MoveOrderResult OrderMove(Location<TUnit> location);
		protected override State OnUpdate(){
			if (!Brain.Unit.IsMoving){
				CurrentState = Brain.Unit.Location == targetLocation ? State.Success : State.Failure;
			}
			return CurrentState;
		}
	}
}
