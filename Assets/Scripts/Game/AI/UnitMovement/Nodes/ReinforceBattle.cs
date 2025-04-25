using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "ReinforceBattle", menuName = "ScriptableObjects/AI/Nodes/ReinforceBattle")]
	public class ReinforceBattle : MilitaryUnitNode<Regiment> {
		private Province target;
		protected override void OnStart(){
			base.OnStart();
			foreach (ProvinceLink link in Unit.Province.Links){
				if (link.Target.IsSea){
					continue;
				}
				LandLocation armyLocation = link.Target.Land.ArmyLocation;
				if (!Brain.IsReinforceableBattleOngoing(armyLocation)){
					continue;
				}
				Blackboard.SetValue(Brain.Target, link.Target);
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
