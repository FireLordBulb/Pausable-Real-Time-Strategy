using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Army> {
		private void Awake(){
			Branch = new Army();
		}
		protected override Location<Army> GetLocation(Province previousProvince, Province nextProvince){
			return nextProvince.Land.ArmyLocation;
		}
		protected override (Location<Army>, int) CalculatePathLocation(){
			Province previousProvince = PathToTarget[PathIndex-1];
			Province nextProvince = PathToTarget[PathIndex];
			ProvinceLink link = previousProvince[nextProvince.ColorKey];
			float terrainSpeedMultiplier = 1+0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier);
			return (GetLocation(previousProvince, nextProvince), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
	}
}