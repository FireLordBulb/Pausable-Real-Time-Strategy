using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "ConstructShip", menuName = "ScriptableObjects/AI/Tasks/ConstructShip")]
	public class ConstructShip : Task {
		[SerializeField] private int enoughShipsPriority;
		[SerializeField] private int maxShipsPriority;
		[Space]
		[SerializeField] private ShipType shipType;
		[SerializeField] private ShipType capType;
		[SerializeField] private float enoughShipsPerCoast;
		[SerializeField] private float maxShipsPerCoast;
	
		private Harbor constructionHarbor;
		
		protected override int CurrentPriority(){
			int shipCount = Country.Ships.Count(ship => ship.Type == shipType);
			int cap = Country.Ships.Count(ship => ship.Type == capType);
			int coastCount = Country.Provinces.Count(land => land.Province.IsCoast);
			if (coastCount*maxShipsPerCoast <= shipCount){
				return maxShipsPriority;
			}
			if (coastCount*enoughShipsPerCoast <= shipCount || cap <= shipCount){
				return enoughShipsPriority;
			}
			return defaultPriority;
		}
		internal override bool CanBePerformed(){
			// Refuse to build any regiments if at max per province.
			if (Priority == maxShipsPriority){
				return false;
			}
			if (!ConstructionHarborIsValid()){
				constructionHarbor = GetConstructionHarbor();
			}
			return constructionHarbor != null && shipType.CanBeBuiltBy(Country);
		}
		internal override void Perform(){
			if (constructionHarbor == null){
				return;
			}
			Country.TryStartConstructingFleet(shipType, constructionHarbor);
		}

		private bool ConstructionHarborIsValid(){
			return constructionHarbor != null && constructionHarbor.Land.Owner == Country && !constructionHarbor.Land.IsOccupied && IsAvailable(constructionHarbor);
		}
		private Harbor GetConstructionHarbor(){
			foreach (Land land in Country.Provinces){
				if (land.IsOccupied || !land.Province.IsCoast){
					continue;
				}
				foreach (ProvinceLink link in land.Province.Links){
					if (link is ShallowsLink shallowsLink && IsAvailable(shallowsLink.Harbor)){
						return shallowsLink.Harbor;
					}
				}
			}
			return null;
		}
		private bool IsAvailable(Harbor harbor){
			return harbor.Units.All(ship => ship.Owner == Controller.Country);
		}
	}
}
