using UnityEngine;

namespace Simulation.Military {
	public class SeaLocation : Location<Navy> {
		public readonly Sea Sea;

		public override Vector3 WorldPosition => Sea.transform.position;
		
		public SeaLocation(Sea sea){
			Sea = sea;
		}
	}
}
