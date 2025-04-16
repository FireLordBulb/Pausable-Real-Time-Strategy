using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FindSiegeTarget", menuName = "ScriptableObjects/AI/Nodes/FindSiegeTarget")]
	public class FindSiegeTarget : MilitaryUnitNode<Regiment> {
		private Country regimentCountry;
		private Country enemyCountry;
		protected override void OnStart(){
			base.OnStart();
			regimentCountry = Brain.Unit.Owner;
			enemyCountry = Tree.Blackboard.GetValue<Country>(Brain.EnemyCountry, null);
			
			Land currentLand = Brain.Unit.Province.Land;
			if (IsGoodSiegeTarget(currentLand, out _)){
				SetTarget(currentLand.Province);
				return;
			}
			IReadOnlyList<Land> provinces = Brain.Controller.GetClosestProvinces(enemyCountry);
			foreach (Land land in provinces){
				if (IsGoodSiegeTarget(land, out List<ProvinceLink> path)){
					SetTarget(path[0].Target);
					return;
				}
			}
			Tree.Blackboard.RemoveValue(Brain.Target);
			CurrentState = State.Failure;
		}
		private bool IsGoodSiegeTarget(Land land, out List<ProvinceLink> path){
			path = null;
			if (land.Controller == regimentCountry || land.Owner != enemyCountry && land.Owner != regimentCountry){
				return false;
			}
			if (Brain.Controller.HasBesiegerAlready(land, Brain.Unit)){
				return false;
			}
			path = Brain.Unit.GetPathTo(land.ArmyLocation, link => {
				bool canEnter = Regiment.LinkEvaluator(link, false, regimentCountry);
				return canEnter && !Brain.Controller.ShouldAvoidArmyAt(link.Target, Brain.Unit);
			});
			return path != null;
		}
		private void SetTarget(Province province){
			Tree.Blackboard.SetValue(Brain.Target, province);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
	}
}
