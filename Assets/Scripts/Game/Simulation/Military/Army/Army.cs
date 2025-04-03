namespace Simulation.Military {
	public class Army : Branch {
		public override bool LinkEvaluator(ProvinceLink link){
			return link is LandLink;
		}
		public override string CreatingVerb => "Recruiting";
	}
}
