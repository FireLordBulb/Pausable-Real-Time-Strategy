using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Unit<T> : MonoBehaviour where T : Branch {
		protected T Branch;
		
		public UnitType<T> Type {get; protected set;}
		public Country Owner {get; protected set;}
		public int BuildDaysLeft {get; private set;}
		public bool IsBuilt {get; private set;}
		public Location<T> Location {get; private set;}

		internal static bool TryStartBuilding(UnitType<T> type, Location<T> buildLocation, Country owner){
			if (!type.CanBeBuiltBy(owner)){
				return false;
			}
			Unit<T> unit = Instantiate(type.Prefab, buildLocation.WorldPosition, Quaternion.identity, owner.MilitaryUnitParent);
			unit.Owner = owner;
			unit.Location = buildLocation;
			unit.Location.Units.Add(unit);
			unit.Type = type;
			unit.Type.ConsumeBuildCostFrom(unit.Owner);
			
			if (unit.Type.DaysToBuild == 0){
				unit.FinishBuilding();
				return true;
			}
			unit.BuildDaysLeft = type.DaysToBuild;
			Calendar.Instance.OnDayTick.AddListener(unit.TickBuild);
			return true;
		}
		
		private void TickBuild(){
			BuildDaysLeft--;
			if (BuildDaysLeft == 0){
				FinishBuilding();
			}
		}
		private void FinishBuilding(){
			IsBuilt = true;
			Calendar.Instance.OnDayTick.RemoveListener(TickBuild);
		}

		internal bool TryMoveTo(Location<T> destination){
			if (!IsBuilt){
				return false;
			}
			List<Province> path = GraphAlgorithms<Province, ProvinceLink>.FindShortestPath_AStar(Location.Province.Graph, Location.Province, destination.Province, Branch.LinkEvaluator);
			if (path == null){
				return false;
			}
			Location.Units.Remove(this);
			destination.Units.Add(this);
			Location = destination;
			transform.position = Location.WorldPosition;
			print($"Moved {path.Count-1} provinces.");
			return true;
		}
	}
}
