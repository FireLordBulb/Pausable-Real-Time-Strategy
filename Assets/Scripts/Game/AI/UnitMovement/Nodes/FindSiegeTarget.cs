using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FindSiegeTarget", menuName = "ScriptableObjects/AI/Nodes/FindSiegeTarget")]
	public class FindSiegeTarget : MilitaryUnitNode<Regiment> {
		private Country regimentCountry;
		private WarEnemy warEnemy;
		protected override void OnStart(){
			base.OnStart();
			regimentCountry = Brain.Unit.Owner;
			warEnemy = Tree.Blackboard.GetValue<WarEnemy>(Brain.EnemyCountry, null);
			
			Land currentLand = Brain.Unit.Province.Land;
			if (IsGoodSiegeTarget(currentLand, out _)){
				SetTarget(currentLand.ArmyLocation);
				return;
			}
			IReadOnlyList<Land> provinces = warEnemy.ClosestProvinces;
			foreach (Land land in provinces){
				if (IsGoodSiegeTarget(land, out List<ProvinceLink> path)){
					SetTarget(path[0].Target.Land.ArmyLocation);
					return;
				}
			}
			Tree.Blackboard.RemoveValue(Brain.Target);
			CurrentState = State.Failure;
		}
		private bool IsGoodSiegeTarget(Land land, out List<ProvinceLink> path){
			path = null;
			if (land.Controller == regimentCountry || land.Owner != warEnemy.Country && land.Owner != regimentCountry){
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
		private void SetTarget(Location<Regiment> location){
			Tree.Blackboard.SetValue(Brain.Target, location);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
	}
}
