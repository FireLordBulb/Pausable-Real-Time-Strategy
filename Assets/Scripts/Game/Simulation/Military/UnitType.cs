using UnityEngine;

namespace Simulation.Military {
	public abstract class UnitType<T> : ScriptableObject where T : Branch {
		[SerializeField] private Unit<T> prefab;
		[SerializeField] private int daysToBuild;
		
		public Unit<T> Prefab => prefab;
		public int DaysToBuild => daysToBuild;
		
		public abstract bool CanBeBuiltBy(Country owner);
	}
}
