namespace Simulation {
	public class CoastLink : ProvinceLink {
		public readonly Military.Harbor Harbor = new();
		public CoastLink(Province source, Province target, int segmentIndex) : base(source, target, segmentIndex){}
	}
}