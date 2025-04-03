using UnityEngine;

namespace Player {
	public class ShipClickCollider : MonoBehaviour {
		[SerializeField] private Simulation.Military.Ship ship;
		public Simulation.Military.Ship Ship => ship;
	}
}