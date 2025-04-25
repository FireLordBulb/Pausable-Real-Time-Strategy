using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Unit<TUnit> : MonoBehaviour, IUnit where TUnit : Unit<TUnit> {
		[Header("Movement")]
		[SerializeField] private float movementSpeed;
		[SerializeField] private float worldSpaceSpeed;
		[SerializeField] private float worldSpaceMoveDirectionOffset;
		[SerializeField] private float worldSpaceMaxOffsetProportion;
		[Header("Battle Randomness")]
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
		public Province Province => Location.Province;
		protected float MovementSpeed => movementSpeed;
		public abstract string CreatingVerb {get;}

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
			unit.StartBuilding();
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
				OnWorldPositionChanged();
			}
		}
		protected virtual void OnWorldPositionChanged(){}
		
		private void StartBuilding(){
			BuildDaysLeft = Type.DaysToBuild;
			Province.Calendar.OnDayTick.AddListener(TickBuild);
		}
		private void TickBuild(){
			BuildDaysLeft--;
			if (BuildDaysLeft <= 0){
				FinishBuilding();
			}
		}
		private void FinishBuilding(){
			// Something is wrong with the listeners causing this to be called multiple times, guard clause used as a dirty fix.
			if (IsBuilt){
				return;
			}
			IsBuilt = true;
			Province.Calendar.OnDayTick.RemoveListener(TickBuild);
			OnFinishBuilding();
			Province.Calendar.OnDayTick.AddListener(OnDayTick);
		}
		protected abstract void OnFinishBuilding();
		private void OnDayTick(){
			if (!IsMoving){
				return;
			}
			DaysToNextLocation--;
			if (0 < DaysToNextLocation){
				return;
			}
			if (!TryValidateNextLocation()){
				return;
			}
			Location.Remove(self);
			worldPositionsOnPath.Enqueue(WorldPositionBetweenLocations());
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
			if (NextLocation != null && GetLocation(path[0]) == NextLocation){
				PathIndex = 0;
			} else {
				PathIndex = -1;
				NextPathLocation();
			}
			Location.UpdateListeners();
			return IsMoving ? MoveOrderResult.Success : MoveOrderResult.DestinationUnusable;
		}
		public List<ProvinceLink> GetPathTo(Location<TUnit> end){
			return GetPath(Location, end, LinkEvaluator);
		}
		public List<ProvinceLink> GetPathTo(Location<TUnit> end, GraphAlgorithms<Province, ProvinceLink>.LinkEvaluator linkEvaluator){
			return GetPath(Location, end, linkEvaluator);
		}
		public static List<ProvinceLink> GetPath(Location<TUnit> start, Location<TUnit> end, GraphAlgorithms<Province, ProvinceLink>.LinkEvaluator linkEvaluator){
			List<Province> nodes = GraphAlgorithms<Province, ProvinceLink>.FindShortestPath_AStar(start.SearchProvince.Graph, start.SearchProvince, end.SearchProvince, linkEvaluator);
			if (nodes == null){
				return null;
			}
			List<ProvinceLink> path = new(nodes.Count);
			for (int i = 1; i < nodes.Count; i++){
				path.Add(nodes[i-1][nodes[i].ColorKey]);
			}
			start.AdjustPathStart(path);
			end.AdjustPathEnd(path);
			return path;
		}
		private void NextPathLocation(){
			PathIndex++;
			if (!TryValidateNextLocation()){
				return;
			}
			DaysToNextLocation = CalculateTravelDays();

			Vector3 nextWorldPosition = WorldPositionBetweenLocations(); 
			float worldSpaceDistance = Vector3.Distance(Location.WorldPosition, nextWorldPosition);
			float offset = Mathf.Min(worldSpaceMaxOffsetProportion*worldSpaceDistance, worldSpaceMoveDirectionOffset);
			Vector3 offsetPosition = Vector3.MoveTowards(Location.WorldPosition, nextWorldPosition, offset);
			worldPositionsOnPath.Enqueue(offsetPosition);
		}
		private bool TryValidateNextLocation(){
			NextLocation = GetLocation(PathToTarget[PathIndex]);
			if (NextLocation != null){
				return true;
			}
			StopMoving();
			return false;
		}
		protected abstract Vector3 WorldPositionBetweenLocations();
		protected abstract int CalculateTravelDays();
		protected abstract Location<TUnit> GetLocation(ProvinceLink link);
		protected abstract bool LinkEvaluator(ProvinceLink link);
		
		internal void CommanderBattleStartUp(){
			daysUntilReroll = 0;
		}
		internal void CommanderBattleTick(){
			daysUntilReroll--;
			if (0 < daysUntilReroll){
				return;
			}
			daysUntilReroll = Random.Range(minRerollDays, maxRerollDays);
			RandomDamageMultiplier = Random.Range(1, 1+maxDamageBoost);
		}
		// Pass the location in since battles can end from a unit changing to a different location.
		internal abstract void CommanderOnBattleEnd(bool didWin, Location<TUnit> location);
		internal abstract BattleResult DoBattle(List<TUnit> defenders, List<TUnit> attackers);
		internal abstract void OnBattleEnd(bool didWin);
		internal abstract void StackWipe();
		
		public void OnSelect(){}
		public void OnDeselect(){}
	}

	// Public interface of Unit for everything non-generic.
	public interface IUnit : ISelectable {
		public Country Owner {get;}
		public int BuildDaysLeft {get;}
		public bool IsBuilt {get;}
		public int DaysToNextLocation {get;}
		public bool IsRetreating {get;}
		public bool IsMoving {get;}
		public Province Province {get;}
		public string CreatingVerb {get;}
	}
}
