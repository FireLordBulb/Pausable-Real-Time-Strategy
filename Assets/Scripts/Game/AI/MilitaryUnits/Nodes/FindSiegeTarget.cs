using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FindSiegeTarget", menuName = "ScriptableObjects/AI/Nodes/FindSiegeTarget")]
	public class FindSiegeTarget : MilitaryUnitNode<Regiment> {
		protected override void OnStart(){
			base.OnStart();
			Country enemyCountry = Tree.Blackboard.GetValue<Country>(Brain.EnemyCountry, null);
			IReadOnlyList<Province> provinces = Brain.Controller.GetClosestProvinces(enemyCountry);
			Province target = Brain.Unit.Province;
			foreach (Province province in provinces){
				if (target.Land.Owner == enemyCountry && target.Land.Occupier != Brain.Unit.Owner){
					break;
				}
				target = province;
			}
			Tree.Blackboard.SetValue(Brain.Target, target);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
		protected override void OnStop(){}
	}
}
