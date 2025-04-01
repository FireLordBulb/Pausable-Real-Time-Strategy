namespace Simulation {
	// A link to coastal land from the sea.
	public class CoastLink : ProvinceLink {
		public readonly Military.Harbor Harbor = new();
		public Sea Sea => Source.Sea;
		public Land Land => Target.Land;
		public CoastLink(Province source, Province target, int segmentIndex) : base(source, target, segmentIndex){}
	}
}