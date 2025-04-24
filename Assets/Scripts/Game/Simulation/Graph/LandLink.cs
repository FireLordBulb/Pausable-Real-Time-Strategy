namespace Simulation {
	public class LandLink : ProvinceLink {
		public Land SourceLand => Source.Land;
		public Land TargetLand => Target.Land;
		internal LandLink(Province source, Province target, int segmentIndex) : base(source, target, segmentIndex){}
	}
}
