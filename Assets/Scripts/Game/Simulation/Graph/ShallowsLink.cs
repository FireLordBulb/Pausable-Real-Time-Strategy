using System;
using UnityEngine;

namespace Simulation {
	// A link to shallow sea from land.
	public class ShallowsLink : ProvinceLink {
		public Land Land => Source.Land;
		public Sea Sea => Target.Sea;
		public Military.Harbor Harbor => ((CoastLink)Reverse).Harbor;
		internal ShallowsLink(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter) : base(source, target, startIndex, endIndex, worldSpaceConverter){}
	}
}
