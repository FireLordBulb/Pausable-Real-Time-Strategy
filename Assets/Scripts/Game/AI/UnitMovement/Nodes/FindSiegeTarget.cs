using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FindSiegeTarget", menuName = "ScriptableObjects/AI/Nodes/FindSiegeTarget")]
	public class FindSiegeTarget : MilitaryUnitNode<Regiment> {
		private Country regimentCountry;
		private WarEnemy warEnemy;
		private IReadOnlyList<Land> provinces;
		protected override void OnStart(){
			base.OnStart();
			regimentCountry = Unit.Owner;
			WarEnemy newWarEnemy = Blackboard.GetValue<WarEnemy>(Brain.EnemyCountry, null);
			if (warEnemy != newWarEnemy){
				warEnemy = newWarEnemy;
				provinces = warEnemy.ClosestProvinces;
			}
			Land currentLand = Unit.Province.Land;
			if (IsGoodSiegeTarget(currentLand, out _)){
				SetTarget(currentLand.ArmyLocation);
				return;
			}
			// If a regiment has been pushed out of an overseas land chunk (because of pose-battle retreat or a peace treaty with a third party)
			// it should default back to trying to occupy the warEnemy closest provinces connected by land to its owning country.
			if (currentLand.Owner != warEnemy.Country){
				provinces = warEnemy.ClosestProvinces;
			}
			foreach (Land land in provinces){
				if (IsGoodSiegeTarget(land, out List<ProvinceLink> path)){
					SetTarget(path[0].Target.Land.ArmyLocation);
					return;
				}
			}
			if (Controller.HasCoast && warEnemy.HasOverseasLand){
				if (Unit.Location is TransportDeck deck){
					Province province = deck.Province;
					if (province.IsLand && province.Land.Owner == warEnemy.Country){
						SetTarget(province.Land.ArmyLocation);
						provinces = warEnemy.GetLandChunk(province.Land);
						return;
					}
				} else foreach (Ship ship in Country.Ships){
					if (ship is not Transport transport || !transport.CanRegimentBoard(Unit) || ship.Location is not Harbor harbor || harbor.Land.Owner != Country){
						continue;
					}
					WarEnemy shipEnemy = Controller.GetBrain(ship).Tree.Blackboard.GetValue<WarEnemy>(Brain.EnemyCountry, null);
					if (shipEnemy != warEnemy){
						continue;
					}
					List<ProvinceLink> path = GetPathTo(transport.Deck);
					if (path == null){
						continue;
					}
					SetTarget(Unit.GetLocation(path[0], transport.Deck));
					return;
				}
			}
			Blackboard.RemoveValue(Brain.Target);
			CurrentState = State.Failure;
		}
		private bool IsGoodSiegeTarget(Land land, out List<ProvinceLink> path){
			path = null;
			if (land.Controller == regimentCountry || land.Owner != warEnemy.Country && land.Owner != regimentCountry){
				return false;
			}
			if (Controller.HasBesiegerAlready(land, Unit)){
				return false;
			}
			path = GetPathTo(land.ArmyLocation);
			return path != null;
		}
		private List<ProvinceLink> GetPathTo(Location<Regiment> location){
			return Unit.GetPathTo(location, link => {
				bool canEnter = Regiment.LinkEvaluator(link, false, regimentCountry);
				return canEnter && !Controller.ShouldAvoidArmyAt(link.Target, Unit);
			});
		}
		private void SetTarget(Location<Regiment> location){
			Blackboard.SetValue(Brain.Target, location);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
	}
}
