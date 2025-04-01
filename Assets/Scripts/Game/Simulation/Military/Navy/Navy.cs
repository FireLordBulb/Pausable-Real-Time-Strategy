namespace Simulation.Military {
	public class Navy : Branch {
		public override bool LinkEvaluator(ProvinceLink link){
			return link is SeaLink or CoastLink;
		}
	}
}
