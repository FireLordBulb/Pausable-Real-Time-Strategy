using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public class Harbor : Location<Navy> {
		public readonly Sea Sea;
		public readonly Land Land;
		private readonly CoastLink coast;
		
		public override string Name {get;}
		
		public override Province SearchTargetProvince => Sea.Province;
		public override Province Province => Land.Province;
		// TODO: Replace with intersection-point of straight line between pivots and the outline segment.
		public override Vector3 WorldPosition => 0.5f*(Sea.transform.position+Land.transform.position);
		
		public Harbor(CoastLink coastLink){
			Sea = coastLink.Sea;
			Land = coastLink.Land;
			coast = coastLink;
			Name = $"Harbor in {Land.Province}";
		}
		public override void AdjustPath(List<ProvinceLink> path){
			path.Add(coast);
		}
	}
}
