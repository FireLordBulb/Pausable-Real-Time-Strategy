using System;
using UnityEngine;

namespace Simulation {
	// A link to coastal land from the sea.
	public class CoastLink : ProvinceLink {
		public readonly Military.Harbor Harbor;
		public Sea Sea => Source.Sea;
		public Land Land => Target.Land;
		internal CoastLink(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter) : base(source, target, startIndex, endIndex, worldSpaceConverter){
			Harbor = new Military.Harbor(this);
		}
	}
}
