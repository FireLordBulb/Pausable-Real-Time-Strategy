using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "ReturnToHarbor", menuName = "ScriptableObjects/AI/Nodes/ReturnToHarbor")]
	public class ReturnToHarbor : MilitaryUnitNode<Ship> {
		protected override void OnStart(){
			base.OnStart();
			Harbor harbor = Blackboard.GetValue<Harbor>(Brain.Harbor, null);
			if (harbor == null){
				CurrentState = State.Failure;
				return;
			}
			Blackboard.SetValue(Brain.Target, harbor);
			CurrentState = State.Success;
		}
	}
}
