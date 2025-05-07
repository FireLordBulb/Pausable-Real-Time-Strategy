using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "KeepBesieging", menuName = "ScriptableObjects/AI/Nodes/KeepBesieging")]
	public class KeepBesieging : MilitaryUnitNode<Regiment> {
		private Province target;
		protected override void OnStart(){
			base.OnStart();
			CurrentState = CannotBesiege() ? State.Failure : State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
		private bool CannotBesiege(){
			return Unit.Location is not LandLocation{SiegeIsOngoing: true} landLocation || landLocation.IsBattleOngoing || Controller.HasBesiegerAlready(landLocation.Land, Unit);
		}
	}
}
