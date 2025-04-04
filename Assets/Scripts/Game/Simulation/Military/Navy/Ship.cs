using UnityEngine;

namespace Simulation.Military {
	public class Ship : Unit<Navy> {
		private void Awake(){
			Branch = new Navy();
		}
		protected override Location<Navy> GetLocation(ProvinceLink link){
			return link switch {
				CoastLink coastLink => coastLink.Harbor,
				ShallowsLink shallowsLink => shallowsLink.Sea.NavyLocation,
				_ => link.Target.Sea.NavyLocation
			};
		}
		protected override (Location<Navy>, int) CalculatePathLocation(){
			ProvinceLink link = PathToTarget[PathIndex];
			float terrainSpeedMultiplier = 1 + link switch {
				CoastLink coastLink => coastLink.Sea.Province.Terrain.MoveSpeedModifier,
				ShallowsLink shallowsLink => shallowsLink.Sea.Province.Terrain.MoveSpeedModifier,
				_ => 0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier)
			};
			return (GetLocation(link), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
		
		public override void StackWipe(){
			Owner.RemoveShip(this);
			Destroy(gameObject);
		}
	}
}