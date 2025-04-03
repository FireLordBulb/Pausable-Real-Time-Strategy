using UnityEngine;

namespace Simulation.Military {
	public class Ship : Unit<Navy> {
		private void Awake(){
			Branch = new Navy();
		}
		protected override Location<Navy> GetLocation(Province previousProvince, Province nextProvince){
			ProvinceLink link = previousProvince[nextProvince.ColorKey];
			return link switch {
				CoastLink coastLink => coastLink.Harbor,
				ShallowsLink shallowsLink => shallowsLink.Sea.NavyLocation,
				_ => nextProvince.Sea.NavyLocation
			};
		}
		protected override (Location<Navy>, int) CalculatePathLocation(){
			Province previousProvince = PathToTarget[PathIndex-1];
			Province nextProvince = PathToTarget[PathIndex];
			ProvinceLink link = previousProvince[nextProvince.ColorKey];
			float terrainSpeedMultiplier = 1 + link switch {
				CoastLink coastLink => coastLink.Sea.Province.Terrain.MoveSpeedModifier,
				ShallowsLink shallowsLink => shallowsLink.Sea.Province.Terrain.MoveSpeedModifier,
				_ => 0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier)
			};
			return (GetLocation(previousProvince, nextProvince), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
	}
}