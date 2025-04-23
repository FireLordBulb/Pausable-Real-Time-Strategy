using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public class Harbor : Location<Ship> {
		public readonly Sea Sea;
		public readonly Land Land;
		private readonly CoastLink coast;
		
		public override string Name {get;}
		
		public override Province SearchTargetProvince => Sea.Province;
		public override Province Province => Land.Province;
		public override Vector3 WorldPosition => coast.WorldPosition;
		
		public Harbor(CoastLink coastLink){
			Sea = coastLink.Sea;
			Land = coastLink.Land;
			coast = coastLink;
			Name = $"Harbor in {Land.Province.Name}";
		}
		public override void AdjustPath(List<ProvinceLink> path){
			path.Add(coast);
		}
	}
}
