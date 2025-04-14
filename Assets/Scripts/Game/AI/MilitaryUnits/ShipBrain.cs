using Simulation.Military;
using UnityEngine;

namespace AI {
	[RequireComponent(typeof(Ship))]
	public class ShipBrain : MilitaryUnitBrain<Ship> {}
}
