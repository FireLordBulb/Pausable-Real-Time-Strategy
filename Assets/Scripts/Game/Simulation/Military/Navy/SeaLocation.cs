using UnityEngine;

namespace Simulation.Military {
	public class SeaLocation : Location<Navy> {
		public readonly Sea Sea;

		public override Vector3 WorldPosition => Sea.transform.position;
		public override Province Province => Sea.Province;
		
		public SeaLocation(Sea sea){
			Sea = sea;
		}
	}
}
