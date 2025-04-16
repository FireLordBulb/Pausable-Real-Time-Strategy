using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "KeepBesieging", menuName = "ScriptableObjects/AI/Nodes/KeepBesieging")]
	public class KeepBesieging : MilitaryUnitNode<Regiment> {
		private Province target;
		protected override void OnStart(){
			base.OnStart();
			if (CannotBesiege()){
				CurrentState = State.Failure;
			}
		}
		protected override State OnUpdate(){
			if (CurrentState == State.Running && CannotBesiege()){
				CurrentState = State.Success;
			}
			return CurrentState;
		}
		private bool CannotBesiege(){
			return Brain.Unit.Location is not LandLocation{SiegeIsOngoing: true} landLocation ||
			       landLocation.IsBattleOngoing ||
			       Brain.Controller.HasBesiegerAlready(landLocation.Land, Brain.Unit);
		}
	}
}
