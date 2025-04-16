using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "MoveToSiege", menuName = "ScriptableObjects/AI/Nodes/MoveToSiege")]
	public class MoveToSiege : MilitaryUnitNode<Regiment> {
		private Province nextProvince;
		private int pathIndex;
		protected override void OnStart(){
			base.OnStart();
			List<ProvinceLink> pathToTarget = Tree.Blackboard.GetValue(Brain.PathToTarget, new List<ProvinceLink>());
			if (pathToTarget.Count == 0){
				nextProvince = Brain.Unit.Province;
				return;
			}
			nextProvince = pathToTarget[0].Target;
			MoveOrderResult result = Brain.Controller.Country.MoveRegimentTo(Brain.Unit, nextProvince);
			CurrentState = result == MoveOrderResult.Success ? State.Success : State.Failure;
		}
		protected override State OnUpdate(){
			UpdateCurrentState();
			return CurrentState;
		}
		private void UpdateCurrentState(){
			// Cancel the movement if a big scary army moved into the path.
			if (Brain.Controller.ShouldAvoidArmyAt(nextProvince, Brain.Unit)){
				Brain.Controller.Country.MoveRegimentTo(Brain.Unit, Brain.Unit.Province);
				CurrentState = State.Failure;
				return;
			}
			// Wait for the movement to progress.
			if (Brain.Unit.IsMoving){
				return;
			}
			CurrentState = Brain.Unit.Province == nextProvince ? State.Success : State.Failure;
		}
	}
}
