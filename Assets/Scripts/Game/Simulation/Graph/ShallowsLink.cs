namespace Simulation {
	// A link to shallow sea from land.
	public class ShallowsLink : ProvinceLink {
		public Land Land => Source.Land;
		public Sea Sea => Target.Sea;
		public Military.Harbor Harbor => ((CoastLink)Reverse).Harbor;
		internal ShallowsLink(Province source, Province target, int segmentIndex) : base(source, target, segmentIndex){}
	}
}
