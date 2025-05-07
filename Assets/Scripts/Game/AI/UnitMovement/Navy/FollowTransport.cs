using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FollowTransport", menuName = "ScriptableObjects/AI/Nodes/FollowTransport")]
	public class FollowTransport : MilitaryUnitNode<Ship> {
		protected override void OnStart(){
			base.OnStart();
			WarEnemy warEnemy = Blackboard.GetValue<WarEnemy>(Brain.EnemyCountry, null);
			foreach (ShipBrain shipBrain in Controller.ShipBrains){
				if (shipBrain.Unit is not Transport || shipBrain.Tree.Blackboard.GetValue<WarEnemy>(Brain.EnemyCountry, null) != warEnemy){
					continue;
				}
				Blackboard.SetValue(Brain.Target, shipBrain.Unit.TargetLocation ?? shipBrain.Unit.Location);
				CurrentState = State.Success;
				return;
			}
			CurrentState = State.Failure;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
	}
}
