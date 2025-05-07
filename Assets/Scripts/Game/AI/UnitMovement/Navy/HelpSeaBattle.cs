using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "HelpSeaBattle", menuName = "ScriptableObjects/AI/Nodes/HelpSeaBattle")]
	public class HelpSeaBattle : MilitaryUnitNode<Ship> {
		protected override void OnStart(){
			base.OnStart();
			foreach (ProvinceLink link in Unit.Province.Links){
				if (link is LandLink){
					continue;
				}
				Location<Ship> location = link is CoastLink coastLink ? coastLink.Harbor : link.Target.Sea.NavyLocation;
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
