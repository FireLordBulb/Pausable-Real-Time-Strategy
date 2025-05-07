using Simulation.Military;

namespace AI.Nodes {
	public abstract class SailTargetDecorator : UnitDecorator<Ship> {
		private Location<Ship> targetLocation;
		
		protected override void OnStart(){
			base.OnStart();
			targetLocation = Blackboard.GetValue<Location<Ship>>(Brain.Target, null);
		}
		protected override bool Predicate(){
			return targetLocation != null && IsTargetValid(targetLocation);
		}
		protected abstract bool IsTargetValid(Location<Ship> targetLocation);
		protected override void OnFailure(){
			Country.MoveFleetTo(Unit, Unit.Location);
		}
	}
}
