using System.Collections.Generic;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "InvadeHarbor", menuName = "ScriptableObjects/AI/Nodes/InvadeHarbor")]
	public class InvadeHarbor : MilitaryUnitNode<Ship> {
		private WarEnemy warEnemy;
		protected override void OnStart(){
			base.OnStart();
			warEnemy = Blackboard.GetValue<WarEnemy>(Brain.EnemyCountry, null);
			
			Location<Ship> currentLocation = Unit.Location;
			if (currentLocation is Harbor harbor && harbor.Land.Owner == warEnemy.Country){
				Transport transport = (Transport)Unit;
				if (transport.Deck.Units.Count == 0){
					CurrentState = State.Failure;
				} else {
					SetTarget(currentLocation);
				}
				return;
			}
			List<Land> landChunk = warEnemy.GetAnyBesiegableLandChunk();
			if (landChunk != null){
				Province coastalProvince = landChunk[0].Province;
				foreach (ProvinceLink link in coastalProvince.Links){
					if (link is not ShallowsLink shallowsLink){
						continue;
					}
					SetTarget(shallowsLink.Harbor);
					return;
				}
			}
			CurrentState = State.Failure;
		}
		private void SetTarget(Location<Ship> target){
			Blackboard.SetValue(Brain.Target, target);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
	}
}
