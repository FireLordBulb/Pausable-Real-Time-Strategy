using System;
using UnityEngine;

namespace Simulation {
	// A link to shallow sea from land.
	public class ShallowsLink : HarborLink {
		public override Military.Harbor Harbor => ((CoastLink)Reverse).Harbor;
		public override Land Land => Source.Land;
		public override Sea Sea => Target.Sea;
		internal ShallowsLink(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter) : base(source, target, startIndex, endIndex, worldSpaceConverter){}
	}
}
