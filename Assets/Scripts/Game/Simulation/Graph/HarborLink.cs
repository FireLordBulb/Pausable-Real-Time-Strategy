using System;
using UnityEngine;

namespace Simulation {
	public abstract class HarborLink : ProvinceLink {
		public abstract Military.Harbor Harbor {get;}
		public abstract Land Land {get;}
		public abstract Sea Sea {get;}
		internal HarborLink(Province source, Province target, int startIndex, int endIndex, Func<Vector2, Vector3> worldSpaceConverter) : base(source, target, startIndex, endIndex, worldSpaceConverter){}
	}
}