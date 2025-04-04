using UnityEngine;

namespace Simulation.Military {
	public abstract class UnitType<T> : ScriptableObject where T : Unit<T> {
		[SerializeField] private Unit<T> prefab;
		[SerializeField] private int daysToBuild;
		[SerializeField] protected float goldCost;
		
		public Unit<T> Prefab => prefab;
		public int DaysToBuild => daysToBuild;
		
		public abstract bool CanBeBuiltBy(Country owner);
		public abstract void ConsumeBuildCostFrom(Country owner);
		public abstract string GetCostAsString();
		public abstract string CreatedVerb {get;}
	}
}
