namespace Simulation.Military {
	public abstract class Branch {
		public abstract bool LinkEvaluator(ProvinceLink link);
		public abstract string CreatingVerb {get;}
	}
}
