using Simulation.Military;
using UnityEngine;

namespace AI {
	[RequireComponent(typeof(Regiment))]
	public class RegimentBrain : MilitaryUnitBrain<Regiment> {}
}
