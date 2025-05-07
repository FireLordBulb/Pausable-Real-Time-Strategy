using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "StayInSeaBattle", menuName = "ScriptableObjects/AI/Nodes/StayInSeaBattle")]
	public class StayInSeaBattle : MilitaryUnitNode<Ship> {
		protected override State OnUpdate(){
			if (!Unit.IsRetreating && (!Unit.Location.IsBattleOngoing || SideLacksCombatShip(Unit.Location.DefendingCountry) || SideLacksCombatShip(Unit.Location.AttackingCountry))){
				CurrentState = State.Failure;
			}
			return CurrentState;
		}

		private bool SideLacksCombatShip(Country country){
			return Unit.Location.Units.All(ship => ship.Owner != country || ship is Transport);
		}
	}
}
