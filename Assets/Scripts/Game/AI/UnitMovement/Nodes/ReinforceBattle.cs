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
				Province province = link.Target;
				if (province.IsSea){
					continue;
				}
				Location<Regiment> location = province.Land.ArmyLocation;
				if(!Brain.IsReinforceableBattleOngoing(location)){
					continue;
				}
				Blackboard.SetValue(Brain.Target, location);
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
