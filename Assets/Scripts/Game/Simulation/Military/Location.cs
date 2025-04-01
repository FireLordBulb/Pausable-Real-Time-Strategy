using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Location<T> where T : Branch {
		public readonly List<Unit<T>> Units = new();
		public abstract Province Province {get;}
		public abstract Vector3 WorldPosition {get;}
	}
}
