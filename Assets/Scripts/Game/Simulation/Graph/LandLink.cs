namespace Simulation {
	public class LandLink : ProvinceLink {
		public Land SourceLand => Source.Land;
		public Land TargetLand => Source.Land;
		public LandLink(Province source, Province target, int segmentIndex) : base(source, target, segmentIndex){}
	}
}
