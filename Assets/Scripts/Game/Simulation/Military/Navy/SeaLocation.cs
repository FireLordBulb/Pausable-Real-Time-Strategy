using UnityEngine;

namespace Simulation.Military {
	public class SeaLocation : Location<Ship> {
		public readonly Sea Sea;
		
		public override string Name => Sea.Province.Name;
		public override Province Province => Sea.Province;
		public override Vector3 WorldPosition => Sea.transform.position;
		
		public SeaLocation(Sea sea){
			Sea = sea;
		}
	}
}
