using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Unit<T> : MonoBehaviour where T : Branch {
		[SerializeField] private float movementSpeed;
		[SerializeField] private float worldSpaceSpeed;
		
		protected T Branch;

		private List<Province> pathToTarget;
		private int pathIndex;
		private readonly Queue<Vector3> worldPositionsOnPath = new();
		
		public UnitType<T> Type {get; protected set;}
		public Country Owner {get; protected set;}
		public int BuildDaysLeft {get; private set;}
		public bool IsBuilt {get; private set;}
		public Location<T> Location {get; private set;}
		public Location<T> NextLocation {get; private set;}
		public Location<T> TargetLocation {get; private set;}
		public int DaysToNextLocation {get; private set;}

		public bool IsMoving => pathToTarget != null;
		
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
		
		private void Update(){
			if (0 < worldPositionsOnPath.Count){
				Vector3 target = worldPositionsOnPath.Peek();
				transform.position = Vector3.MoveTowards(transform.position, target, worldSpaceSpeed*Time.deltaTime);
				if (Vector3.Distance(transform.position, target) < Vector3.kEpsilon){
					worldPositionsOnPath.Dequeue();
				}
			}
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
			if (!IsMoving){
				return;
			}
			DaysToNextLocation--;
			if (0 < DaysToNextLocation){
				return;
			}
			Location.Units.Remove(this);
			NextLocation.Units.Add(this);
			Location = NextLocation;
			worldPositionsOnPath.Enqueue(Location.WorldPosition);
			if (ReferenceEquals(NextLocation, TargetLocation)){
				pathToTarget = null;
				NextLocation = null;
				TargetLocation = null;
			} else {
				NextPathIndex();
			}
		}
		
		protected abstract Location<T> GetLocation(Province province);
		
		internal MoveOrderResult MoveTo(Location<T> destination){
			if (!IsBuilt){
				return MoveOrderResult.NotBuilt;
			}
			if (Location == destination){
				pathToTarget = null;
				TargetLocation = null;
				return MoveOrderResult.AlreadyAtDestination;
			}
			List<Province> path = GraphAlgorithms<Province, ProvinceLink>.FindShortestPath_AStar(Location.Province.Graph, Location.Province, destination.Province, Branch.LinkEvaluator);
			if (path == null){
				return MoveOrderResult.NoPath;
			}
			pathToTarget = path;
			TargetLocation = destination;
			pathIndex = 0;
			NextPathIndex();
			return MoveOrderResult.Success;
		}
		private void NextPathIndex(){
			pathIndex++;
			Province nextProvince = pathToTarget[pathIndex];
			NextLocation = GetLocation(nextProvince);
			ProvinceLink link = pathToTarget[pathIndex-1][nextProvince.ColorKey];
			// TODO: Add Terrain.unitSpeedMultiplier
			float terrainSpeedMultiplier = 1+0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier);
			DaysToNextLocation = Mathf.CeilToInt(link.Distance/(movementSpeed*terrainSpeedMultiplier));
		}

		public string CreatingVerb => Branch.CreatingVerb;
	}
}
