using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Unit<T> : MonoBehaviour where T : Branch {
		protected T Branch;

		private List<Province> pathToTarget;
		private int pathIndex;
		private int daysToNextLocation;
		
		public UnitType<T> Type {get; protected set;}
		public Country Owner {get; protected set;}
		public int BuildDaysLeft {get; private set;}
		public bool IsBuilt {get; private set;}
		public Location<T> Location {get; private set;}
		public Location<T> TargetLocation {get; private set;}
		
		public IEnumerable<Province> PathToTarget => pathToTarget;

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
			Calendar.Instance.OnDayTick.AddListener(OnDayTick);
		}
		private void OnDayTick(){
			if (pathToTarget == null){
				return;
			}
			daysToNextLocation--;
			if (0 < daysToNextLocation){
				return;
			}
			Location.Units.Remove(this);
			Location<T> nextLocation = GetLocation(pathToTarget[pathIndex]);
					
			nextLocation.Units.Add(this);
			Location = nextLocation;
			transform.position = Location.WorldPosition;
			if (ReferenceEquals(nextLocation, TargetLocation)){
				pathToTarget = null;
				TargetLocation = null;
			} else {
				NextPathIndex();
			}
		}
		
		protected abstract Location<T> GetLocation(Province province);
		
		internal bool TryMoveTo(Location<T> destination){
			if (!IsBuilt){
				return false;
			}
			List<Province> path = GraphAlgorithms<Province, ProvinceLink>.FindShortestPath_AStar(Location.Province.Graph, Location.Province, destination.Province, Branch.LinkEvaluator);
			if (path == null){
				return false;
			}
			pathToTarget = path;
			TargetLocation = destination;
			pathIndex = 0;
			NextPathIndex();
			return true;
		}
		private void NextPathIndex(){
			pathIndex++;
			ProvinceLink link = pathToTarget[pathIndex-1][pathToTarget[pathIndex].ColorKey];
			// TODO: Add Terrain.unitSpeedMultiplier
			float terrainSpeedMultiplier = 1;//0.5f*(link.Source.Terrain.unitSpeedMultiplier+link.Target.Terrain.unitSpeedMultiplier);
			daysToNextLocation = Mathf.CeilToInt(terrainSpeedMultiplier*link.Distance);
		}
	}
}
