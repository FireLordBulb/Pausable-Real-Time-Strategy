using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "CanMove", menuName = "ScriptableObjects/AI/Nodes/CanMove")]
	public class CanMove : UnitDecorator<Regiment> {
		protected override bool Predicate(){
			return Unit.IsBuilt && !Unit.IsRetreating && IsNotAtSea();
		}
		private bool IsNotAtSea(){
			return Unit.Location is not TransportDeck deck || deck.Transport.Location is Harbor && !deck.Transport.IsMoving;
		}
	}
}
