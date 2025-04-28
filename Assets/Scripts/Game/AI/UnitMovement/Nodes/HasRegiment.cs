using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "HasRegiment", menuName = "ScriptableObjects/AI/Nodes/HasRegiment")]
	public class HasRegiment : UnitDecorator<Ship> {
		private Transport transport;
		protected override void OnStart(){
			base.OnStart();
			transport = (Transport)Unit;
		}
		protected override bool Predicate(){
			return transport.Deck.Units.Count > 0;
		}
	}
}
