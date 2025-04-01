using System.Collections.Generic;

namespace Simulation.Military {
	public abstract class Location<T> where T : Branch {
		private List<Unit<T>> units = new();
	}
}
