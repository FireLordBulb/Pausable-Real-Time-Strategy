using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FindSiegeTarget", menuName = "ScriptableObjects/AI/Nodes/FindSiegeTarget")]
	public class FindSiegeTarget : MilitaryUnitNode<Regiment> {
		protected override void OnStart(){
			base.OnStart();
			Country regimentCountry = Brain.Unit.Owner;
			Country enemyCountry = Tree.Blackboard.GetValue<Country>(Brain.EnemyCountry, null);
			IReadOnlyList<Land> provinces = Brain.Controller.GetClosestProvinces(enemyCountry);
			Land targetLand = Brain.Unit.Province.Land;
			foreach (Land land in provinces){
				if (targetLand.Controller != regimentCountry && (targetLand.Owner == enemyCountry || targetLand.Owner == regimentCountry)){
					break;
				}
				targetLand = land;
			}
			Tree.Blackboard.SetValue(Brain.Target, targetLand.Province);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
		protected override void OnStop(){}
	}
}
