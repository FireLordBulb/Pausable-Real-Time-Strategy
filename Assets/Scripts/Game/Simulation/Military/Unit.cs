using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Unit<T> : MonoBehaviour where T : Unit<T> {
		[SerializeField] private float movementSpeed;
		[SerializeField] private float worldSpaceSpeed;
		
		private readonly Queue<Vector3> worldPositionsOnPath = new();
		
		public UnitType<T> Type {get; protected set;}
		public Country Owner {get; protected set;}
		public int BuildDaysLeft {get; private set;}
		public bool IsBuilt {get; private set;}
		public Location<T> Location {get; private set;}
		public Location<T> NextLocation {get; private set;}
		public Location<T> TargetLocation {get; private set;}
		public int DaysToNextLocation {get; private set;}
		protected List<ProvinceLink> PathToTarget {get; private set;}
		protected int PathIndex {get; private set;}

		public bool IsMoving => PathToTarget != null;
		protected float MovementSpeed => movementSpeed;
		
		internal static Unit<T> StartCreating(UnitType<T> type, Location<T> buildLocation, Country owner){
			if (!type.CanBeBuiltBy(owner)){
				return null;
			}
			Unit<T> unit = Instantiate(type.Prefab, buildLocation.WorldPosition, Quaternion.identity, owner.MilitaryUnitParent);
			unit.Owner = owner;
			unit.Location = buildLocation;
			unit.Location.Add(unit);
			unit.Type = type;
			unit.gameObject.name = type.name;
			
			type.ConsumeBuildCostFrom(unit.Owner);
			if (type.DaysToBuild == 0){
				unit.FinishBuilding();
				return unit;
			}
			unit.BuildDaysLeft = type.DaysToBuild;
			Calendar.Instance.OnDayTick.AddListener(unit.TickBuild);
			return unit;
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
			Location.Remove(this);
			NextLocation.Add(this);
			Location = NextLocation;
			worldPositionsOnPath.Enqueue(Location.WorldPosition);
			if (ReferenceEquals(NextLocation, TargetLocation)){
				PathToTarget = null;
				NextLocation = null;
				TargetLocation = null;
			} else {
				NextPathLocation();
			}
		}
		
		internal MoveOrderResult MoveTo(Location<T> destination){
			if (!IsBuilt){
				return MoveOrderResult.NotBuilt;
			}
			if (Location == destination){
				PathToTarget = null;
				TargetLocation = null;
				return MoveOrderResult.AlreadyAtDestination;
			}
			List<ProvinceLink> path = GetPathTo(destination);
			if (path == null){
				return MoveOrderResult.NoPath;
			}
			PathToTarget = path;
			TargetLocation = destination;
			if (NextLocation == GetLocation(path[0])){
				PathIndex = 0;
			} else {
				PathIndex = -1;
				NextPathLocation();
			}
			return MoveOrderResult.Success;
		}
		public List<ProvinceLink> GetPathTo(Location<T> end){
			List<Province> nodes = GraphAlgorithms<Province, ProvinceLink>.FindShortestPath_AStar(Location.Province.Graph, Location.Province, end.SearchTargetProvince, LinkEvaluator);
			if (nodes == null){
				return null;
			}
			List<ProvinceLink> path = new(nodes.Count);
			for (int i = 1; i < nodes.Count; i++){
				path.Add(nodes[i-1][nodes[i].ColorKey]);
			}
			end.AdjustPath(path);
			return path;
		}
		private void NextPathLocation(){
			PathIndex++;
			(NextLocation, DaysToNextLocation) = CalculatePathLocation();
		}
		protected abstract (Location<T>, int) CalculatePathLocation();
		protected abstract Location<T> GetLocation(ProvinceLink link);

		public abstract void StackWipe();
		
		public abstract bool LinkEvaluator(ProvinceLink link);
		public abstract string CreatingVerb {get;}
	}
}
