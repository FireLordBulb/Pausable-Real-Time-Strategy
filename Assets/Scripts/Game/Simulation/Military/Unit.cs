using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Unit<TUnit> : MonoBehaviour where TUnit : Unit<TUnit> {
		[SerializeField] private float movementSpeed;
		[SerializeField] private float worldSpaceSpeed;
		[SerializeField] private float worldSpaceMoveDirectionOffset;
		[SerializeField] private float worldSpaceMaxOffsetProportion;
		[Header("Battle randomness")]
		[SerializeField] private float maxDamageBoost;
		[SerializeField] private int minRerollDays;
		[SerializeField] private int maxRerollDays;
		
		// Used to block use of Object.Instantiate outside of the StartCreating factory function. Serialized so it's actually copied from the prefab.
		[SerializeField, HideInInspector] private bool doAllowInstantiate;
		
		private TUnit self;
		private readonly Queue<Vector3> worldPositionsOnPath = new();
		private int daysUntilReroll;
		
		public UnitType<TUnit> Type {get; private set;}
		public Country Owner {get; private set;}
		public int BuildDaysLeft {get; private set;}
		public bool IsBuilt {get; private set;}
		public Location<TUnit> Location {get; private set;}
		public Location<TUnit> NextLocation {get; private set;}
		public Location<TUnit> TargetLocation {get; private set;}
		public int DaysToNextLocation {get; private set;}
		protected List<ProvinceLink> PathToTarget {get; private set;}
		protected int PathIndex {get; private set;}
		protected float RandomDamageMultiplier {get; private set;}
		public bool IsRetreating {get; protected set;}
		
		public bool IsMoving => PathToTarget != null;
		protected float MovementSpeed => movementSpeed;
		
		internal static TUnit StartCreating(UnitType<TUnit> type, Location<TUnit> buildLocation, Country owner){
			if (!type.CanBeBuiltBy(owner)){
				return null;
			}
			foreach (TUnit locationUnit in buildLocation.Units){
				// Can't build a unit where there's an enemy army
				if (locationUnit.Owner != owner){
					return null;
				}
			}
			type.Prefab.doAllowInstantiate = true;
			TUnit unit = Instantiate(type.Prefab, buildLocation.WorldPosition, Quaternion.identity, owner.MilitaryUnitParent);
			type.Prefab.doAllowInstantiate = false;
			unit.self = unit;
			unit.Owner = owner;
			unit.Location = buildLocation;
			unit.Location.Add(unit);
			unit.gameObject.name = type.name;
			unit.Type = type;
			unit.Type.ApplyValuesTo(unit);
			
			type.ConsumeBuildCostFrom(unit.Owner);
			if (type.DaysToBuild == 0){
				unit.FinishBuilding();
				return unit;
			}
			unit.BuildDaysLeft = type.DaysToBuild;
			Calendar.Instance.OnDayTick.AddListener(unit.TickBuild);
			return unit;
		}
		private void Awake(){
			if (doAllowInstantiate){
				return;
			}
			Debug.LogError("ERROR: Object.Instantiate was used to create Military Unit directly! This is not allowed. Use the Unit<TUnit>.StartCreating factory method instead.");
			DestroyImmediate(gameObject);
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
			Location.Remove(self);
			Location = NextLocation;
			if (ReferenceEquals(Location, TargetLocation)){
				StopMoving();
			} else {
				NextPathLocation();
			}
			Location.Add(self);
		}
		internal void StopMoving(){
			PathToTarget = null;
			NextLocation = null;
			TargetLocation = null;
			IsRetreating = false;
			worldPositionsOnPath.Enqueue(Location.WorldPosition);
		}
		
		internal MoveOrderResult MoveTo(Location<TUnit> destination){
			if (IsRetreating){
				return MoveOrderResult.BusyRetreating;
			}
			return SetDestination(destination);
		}
		protected MoveOrderResult SetDestination(Location<TUnit> destination){
			if (!IsBuilt){
				return MoveOrderResult.NotBuilt;
			}
			if (Location == destination){
				StopMoving();
				Location.UpdateListeners();
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
			Location.UpdateListeners();
			return MoveOrderResult.Success;
		}
		protected List<ProvinceLink> GetPathTo(Location<TUnit> end){
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
			float worldSpaceDistance = Vector3.Distance(Location.WorldPosition, NextLocation.WorldPosition);
			float offset = Mathf.Min(worldSpaceMaxOffsetProportion*worldSpaceDistance, worldSpaceMoveDirectionOffset);
			Vector3 offsetPosition = Vector3.MoveTowards(Location.WorldPosition, NextLocation.WorldPosition, offset);
			worldPositionsOnPath.Enqueue(offsetPosition);
		}
		protected abstract (Location<TUnit>, int) CalculatePathLocation();
		protected abstract Location<TUnit> GetLocation(ProvinceLink link);

		public void StartupBattleRandomness(){
			daysUntilReroll = 0;
		}
		public void RerollBattleRandomness(){
			daysUntilReroll--;
			if (0 < daysUntilReroll){
				return;
			}
			daysUntilReroll = Random.Range(minRerollDays, maxRerollDays);
			RandomDamageMultiplier = Random.Range(1, 1+maxDamageBoost);
		}
		public abstract BattleResult DoBattle(List<TUnit> defenders, List<TUnit> attackers);
		public abstract void OnBattleEnd(bool didWin);
		public abstract void StackWipe();
		
		protected abstract bool LinkEvaluator(ProvinceLink link);
		public abstract string CreatingVerb {get;}
	}
}
