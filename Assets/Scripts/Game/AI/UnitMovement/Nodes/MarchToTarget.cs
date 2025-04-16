using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "MoveRegiment", menuName = "ScriptableObjects/AI/Nodes/MoveRegiment")]
	public class MarchToTarget : MilitaryUnitNode<Regiment> {
		private List<ProvinceLink> pathToTarget;
		private Province nextProvince;
		private int pathIndex;
		protected override void OnStart(){
			base.OnStart();
			pathToTarget = Tree.Blackboard.GetValue(Brain.PathToTarget, new List<ProvinceLink>());
			pathIndex = -1;
			nextProvince = Brain.Unit.Province;
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
			// This case can only happen if something besides this node is giving move orders to the regiment, don't do that.
			if (Brain.Unit.Province != nextProvince){
				CurrentState = State.Failure;
				return;
			}
			// Either start moving toward the next province in the path or conclude with State.Success if the path has ended.
			pathIndex++;
			if (pathIndex < pathToTarget.Count){
				nextProvince = pathToTarget[pathIndex].Target;
				// Don't move too the final province of the path if a different army got to the siege first.
				if (pathIndex == pathToTarget.Count-1 && Brain.Controller.HasBesiegerAlready(nextProvince.Land, Brain.Unit)){
					CurrentState = State.Failure;
					return;
				}
				MoveOrderResult result = Brain.Controller.Country.MoveRegimentTo(Brain.Unit, nextProvince);
				if (result != MoveOrderResult.Success){
					CurrentState = State.Failure;
				}
				return;
			}
			CurrentState = State.Success;
		}
	}
}
