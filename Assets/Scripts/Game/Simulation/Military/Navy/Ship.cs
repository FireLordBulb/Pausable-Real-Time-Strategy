using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public class Ship : Unit<Ship> {
		protected override Location<Ship> GetLocation(ProvinceLink link){
			return link switch {
				CoastLink coastLink => coastLink.Harbor,
				ShallowsLink shallowsLink => shallowsLink.Sea.NavyLocation,
				_ => link.Target.Sea.NavyLocation
			};
		}
		protected override (Location<Ship>, int) CalculatePathLocation(){
			ProvinceLink link = PathToTarget[PathIndex];
			float terrainSpeedMultiplier = 1 + link switch {
				CoastLink coastLink => coastLink.Sea.Province.Terrain.MoveSpeedModifier,
				ShallowsLink shallowsLink => shallowsLink.Sea.Province.Terrain.MoveSpeedModifier,
				_ => 0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier)
			};
			return (GetLocation(link), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
		
		public override BattleResult DoBattle(List<Ship> defenders, List<Ship> attackers){
			return BattleResult.DefenderWon;
		}
		public override void OnBattleEnd(bool didWin){
			if (!didWin){
				StackWipe();
			}
		}
		public override void StackWipe(){
			Owner.RemoveShip(this);
			Destroy(gameObject);
		}
		
		protected override bool LinkEvaluator(ProvinceLink link){
			return link is SeaLink or CoastLink or ShallowsLink;
		}
		public override string CreatingVerb => "Constructing";
	}
}