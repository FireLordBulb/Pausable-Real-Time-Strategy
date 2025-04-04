using UnityEngine;

namespace Simulation.Military {
	public class LandLocation : Location<Regiment> {
		public readonly Land Land;
		
		public override string Name => Province.Name;
		public override Province Province => Land.Province;
		public override Vector3 WorldPosition => Land.transform.position;
		
		public LandLocation(Land land){
			Land = land;
		}
	}
}
