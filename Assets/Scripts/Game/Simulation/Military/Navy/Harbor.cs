using UnityEngine;

namespace Simulation.Military {
	public class Harbor : Location<Navy> {
		public readonly Sea Sea;
		public readonly Land Land;
		
		public override string Name {get;}
		// TODO: Replace with intersection-point of straight line between pivots and the outline segment.
		public override Vector3 WorldPosition => 0.5f*(Sea.transform.position+Land.transform.position);
		public override Province Province => Land.Province;
		
		public Harbor(Sea sea, Land land){
			Sea = sea;
			Land = land;
			Name = $"Harbor in {Land.Province}";
		}
	}
}
