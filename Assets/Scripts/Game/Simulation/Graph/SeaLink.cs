namespace Simulation {
	public class SeaLink : ProvinceLink {
		public Sea SourceSea => Source.Sea;
		public Sea TargetSea => Target.Sea;
		public SeaLink(Province source, Province target, int segmentIndex) : base(source, target, segmentIndex){}
	}
}
