using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "MoveToTarget", menuName = "ScriptableObjects/AI/Nodes/MoveToTarget")]
	public class MoveToTarget : MilitaryUnitNode<Regiment> {
		private Province targetProvince;
		protected override void OnStart(){
			base.OnStart();
			targetProvince = Tree.Blackboard.GetValue<Province>(Brain.Target, null);
			MoveOrderResult result = targetProvince == null ? MoveOrderResult.InvalidTarget : Brain.Controller.Country.MoveRegimentTo(Brain.Unit, targetProvince.Land.ArmyLocation);
			CurrentState = result == MoveOrderResult.Success ? State.Running : State.Failure;
		}
		protected override State OnUpdate(){
			if (!Brain.Unit.IsMoving){
				CurrentState = Brain.Unit.Province == targetProvince ? State.Success : State.Failure;
			}
			return CurrentState;
		}
	}
}
