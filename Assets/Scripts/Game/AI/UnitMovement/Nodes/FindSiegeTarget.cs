using System.Collections.Generic;
using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FindSiegeTarget", menuName = "ScriptableObjects/AI/Nodes/FindSiegeTarget")]
	public class FindSiegeTarget : MilitaryUnitNode<Regiment> {
		[SerializeField] private float maxStrengthMultiplier;
		
		private Country regimentCountry;
		private Country enemyCountry;
		protected override void OnStart(){
			base.OnStart();
			regimentCountry = Brain.Unit.Owner;
			enemyCountry = Tree.Blackboard.GetValue<Country>(Brain.EnemyCountry, null);
			IReadOnlyList<Land> provinces = Brain.Controller.GetClosestProvinces(enemyCountry);
			Land currentLand = Brain.Unit.Province.Land;
			if (IsGoodSiegeTarget(currentLand)){
				SetTarget(currentLand);
				return;
			}
			foreach (Land land in provinces){
				if (IsGoodSiegeTarget(land)){
					SetTarget(land);
					return;
				}
			}
			Tree.Blackboard.RemoveValue(Brain.Target);
			CurrentState = State.Failure;
		}
		private bool IsGoodSiegeTarget(Land land){
			if (land.Controller == regimentCountry || land.Owner != enemyCountry && land.Owner != regimentCountry){
				return false;
			}
			List<ProvinceLink> path = Brain.Unit.GetPathTo(land.ArmyLocation, link => {
				bool canEnter = Regiment.LinkEvaluator(link, false, regimentCountry);
				if (!canEnter){
					return false;
				}
				IReadOnlyList<Regiment> unitsAtLocation = link.Target.Land.ArmyLocation.Units;
				if (unitsAtLocation.All(unit => unit.Owner == regimentCountry)){
					return true;
				}
				Regiment[] enemyRegiments = unitsAtLocation.Where(unit => unit.Owner != regimentCountry).ToArray();
				return AIController.RelativeStrength(Brain.Unit, enemyRegiments) < maxStrengthMultiplier;
			});
			return path != null;
		}
		private void SetTarget(Land land){
			Tree.Blackboard.SetValue(Brain.Target, land.Province);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
	}
}
