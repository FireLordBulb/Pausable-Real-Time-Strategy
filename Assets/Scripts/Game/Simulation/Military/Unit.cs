using UnityEngine;

namespace Simulation.Military {
	public class Unit<T> : MonoBehaviour where T : Branch {
		private T branch;
		private Location<T> location;

		public T Branch => branch;
		public Location<T> Location => location;
	}
}
