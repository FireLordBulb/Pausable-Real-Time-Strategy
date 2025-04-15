using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "MoveRegiment", menuName = "ScriptableObjects/AI/Nodes/MoveRegiment")]
	public class MarchToTarget : MilitaryUnitNode<Regiment> {
		private Province target;
		protected override void OnStart(){
			base.OnStart();
			target = Tree.Blackboard.GetValue<Province>(Brain.Target, null);
			MoveOrderResult result = Brain.Controller.Country.MoveRegimentTo(Brain.Unit, target);
			CurrentState = result == MoveOrderResult.Success ? State.Running : State.Failure;
		}
		protected override State OnUpdate(){
			if (!Brain.Unit.IsMoving){
				CurrentState = Brain.Unit.Province == target ? State.Success : State.Failure;
			}
			return CurrentState;
		}
	}
}
