using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "SeaBattleIsGoing", menuName = "ScriptableObjects/AI/Nodes/SeaBattleIsGoing")]
	public class SeaBattleIsGoing : UnitDecorator<Ship> {
		private Location<Ship> targetLocation;
		
		protected override void OnStart(){
			base.OnStart();
			targetLocation = Blackboard.GetValue<Location<Ship>>(Brain.Target, null);
		}
		protected override bool Predicate(){
			return targetLocation != null && Brain.IsReinforceableBattleOngoing(targetLocation);
		}
		protected override void OnFailure(){
			Country.MoveFleetTo(Unit, Unit.Location);
		}
	}
}
