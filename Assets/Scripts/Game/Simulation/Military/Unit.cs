using UnityEngine;

namespace Simulation.Military {
	public abstract class Unit<T> : MonoBehaviour where T : Branch {
		public T Branch {get; protected set;}
		public int BuildDaysLeft {get; private set;}
		public bool IsBuilt {get; private set;}
		public Location<T> Location {get; private set;}

		public static bool TryStartBuilding(Country owner, Location<T> buildLocation, UnitType<T> type){
			if (!type.CanBeBuiltBy(owner)){
				return false;
			}
			Unit<T> unit = Instantiate(type.Prefab, buildLocation.WorldPosition, Quaternion.identity, owner.transform);
			buildLocation.Units.Add(unit);
			unit.Location = buildLocation;
			if (type.DaysToBuild == 0){
				unit.FinishBuilding();
				return true;
			}
			unit.BuildDaysLeft = type.DaysToBuild;
			Calendar.Instance.OnDayTick.AddListener(unit.TickBuild);
			return true;
		}
		
		public void TickBuild(){
			BuildDaysLeft--;
			if (BuildDaysLeft == 0){
				FinishBuilding();
			}
		}
		private void FinishBuilding(){
			IsBuilt = true;
			Calendar.Instance.OnDayTick.RemoveListener(TickBuild);
		}
	}
}
