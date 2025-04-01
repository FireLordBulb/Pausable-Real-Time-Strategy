namespace Simulation {
	public class SeaLink : ProvinceLink {
		public Sea SourceSea => Source.Sea;
		public Sea TargetSea => Source.Sea;
		public SeaLink(Province source, Province target, int segmentIndex) : base(source, target, segmentIndex){}
	}
}