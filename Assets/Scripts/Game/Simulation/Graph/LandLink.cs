using System;
using UnityEngine;

namespace Simulation {
	public class LandLink : ProvinceLink {
		public Land SourceLand => Source.Land;
		public Land TargetLand => Target.Land;
		internal LandLink(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter) : base(source, target, startIndex, endIndex, worldSpaceConverter){}
	}
}
