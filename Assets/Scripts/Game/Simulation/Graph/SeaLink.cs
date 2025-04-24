using System;
using UnityEngine;

namespace Simulation {
	public class SeaLink : ProvinceLink {
		public Sea SourceSea => Source.Sea;
		public Sea TargetSea => Target.Sea;
		internal SeaLink(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter) : base(source, target, startIndex, endIndex, worldSpaceConverter){}
	}
}
