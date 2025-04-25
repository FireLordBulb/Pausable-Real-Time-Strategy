using Simulation.Military;

namespace AI.Nodes {
	public abstract class TargetDecorator : UnitDecorator<Regiment> {
		private Location<Regiment> targetLocation;
		
		protected override void OnStart(){
			base.OnStart();
			targetLocation = Blackboard.GetValue<Location<Regiment>>(Brain.Target, null);
		}
		protected override bool Predicate(){
			return targetLocation != null && IsTargetValid(targetLocation);
		}
		protected abstract bool IsTargetValid(Location<Regiment> targetLocation);
		protected override void OnFailure(){
			Country.MoveRegimentTo(Unit, Unit.Location);
		}
	}
}
