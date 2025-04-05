using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Regiment> {
		protected override Location<Regiment> GetLocation(ProvinceLink link){
			return link.Target.Land.ArmyLocation;
		}
		protected override (Location<Regiment>, int) CalculatePathLocation(){
			ProvinceLink link = PathToTarget[PathIndex];
			float terrainSpeedMultiplier = 1+0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier);
			return (GetLocation(link), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
		
		public override BattleResult DefendBattle(Regiment attacker){
			int diceRoll = Random.Range(0, 6);
			if (diceRoll < 3){
				return BattleResult.Ongoing;
			}
			if (diceRoll == 3){
				return BattleResult.AttackerWon;
			}
			return BattleResult.DefenderWon;
		}
		public override void StackWipe(){
			Owner.RemoveRegiment(this);
			Destroy(gameObject);
		}
		
		protected override bool LinkEvaluator(ProvinceLink link){
			return link is LandLink && (Owner == link.Target.Owner || Owner.GetDiplomaticStatus(link.Target.Owner).IsAtWar);
		}
		public override string CreatingVerb => "Recruiting";
	}
}