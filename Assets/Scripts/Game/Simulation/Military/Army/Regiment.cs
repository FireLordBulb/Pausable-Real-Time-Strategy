using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Army> {
		private void Awake(){
			Branch = new Army(this);
		}
		protected override Location<Army> GetLocation(ProvinceLink link){
			return link.Target.Land.ArmyLocation;
		}
		protected override (Location<Army>, int) CalculatePathLocation(){
			ProvinceLink link = PathToTarget[PathIndex];
			float terrainSpeedMultiplier = 1+0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier);
			return (GetLocation(link), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
	}
}