using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "NoAllyToHelp", menuName = "ScriptableObjects/AI/Nodes/NoAllyToHelp")]
	public class NoAllyToHelp : UnitDecorator<Regiment> {
		protected override bool Predicate(){
			foreach (ProvinceLink link in Unit.Province.Links){
				Province province = link.Target;
				if (province.IsLand && Brain.IsReinforceableBattleOngoing(province.Land.ArmyLocation)){
					return false;
				}
			}
			return true;
		}
	}
}
