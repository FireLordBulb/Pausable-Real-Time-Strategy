using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "MoveRegiment", menuName = "ScriptableObjects/AI/Nodes/MoveRegiment")]
	public class MarchToTarget : MilitaryUnitNode<Regiment> {
		private List<ProvinceLink> pathToTarget;
		protected override void OnStart(){
			base.OnStart();
			pathToTarget = Tree.Blackboard.GetValue(Brain.PathToTarget, new List<ProvinceLink>());
			if (pathToTarget.Count == 0){
				return;
			}
			MoveOrderResult result = Brain.Controller.Country.MoveRegimentTo(Brain.Unit, pathToTarget[^1].Target);
			CurrentState = result == MoveOrderResult.Success ? State.Running : State.Failure;
		}
		protected override State OnUpdate(){
			if (!Brain.Unit.IsMoving){
				CurrentState = pathToTarget.Count == 0 || Brain.Unit.Province == pathToTarget[^1].Target ? State.Success : State.Failure;
			}
			return CurrentState;
		}
	}
}
