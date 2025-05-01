using UnityEngine;

namespace Simulation.Military {
	public abstract class UnitType<TUnit> : ScriptableObject where TUnit : Unit<TUnit> {
		[SerializeField] private TUnit prefab;
		[SerializeField] private int daysToBuild;
		[SerializeField] protected float maintenanceCost;
		[SerializeField] protected float goldCost;
		
		public TUnit Prefab => prefab;
		public int DaysToBuild => daysToBuild;
		
		public abstract bool CanBeBuiltBy(Country owner);
		public abstract void ApplyValuesTo(TUnit unit);
		public abstract void ConsumeBuildCostFrom(Country owner);
		public abstract string GetCostAsString();
		public abstract string CreatedVerb {get;}
	}
}
