using UnityEngine;

namespace Simulation.Military {
	public class LandLocation : Location<Army> {
		public readonly Land Land;

		public override Vector3 WorldPosition => Land.transform.position;
		public override Province Province => Land.Province;
		public override string Name => Province.Name;

		public LandLocation(Land land){
			Land = land;
		}
	}
}
