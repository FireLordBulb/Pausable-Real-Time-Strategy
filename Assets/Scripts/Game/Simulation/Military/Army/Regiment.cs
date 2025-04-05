using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Regiment> {
		public float AttackPower {get; private set;}
		public float Toughness {get; private set;}
		public float KillRate {get; private set;}
		public int MaxManpower {get; private set;}
		public int CurrentManpower {get; private set;}
		public int DemoralizedManpower {get; private set;}
		
		internal void Init(float attackPower, float toughness, float killRate, int manpower){
			AttackPower = attackPower;
			Toughness = toughness;
			KillRate = killRate;
			CurrentManpower = MaxManpower = manpower;
			DemoralizedManpower = 0;
		}
		
		protected override Location<Regiment> GetLocation(ProvinceLink link){
			return link.Target.Land.ArmyLocation;
		}
		protected override (Location<Regiment>, int) CalculatePathLocation(){
			ProvinceLink link = PathToTarget[PathIndex];
			float terrainSpeedMultiplier = 1+0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier);
			return (GetLocation(link), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
		
		public override BattleResult DefendBattle(Regiment attacker){
			float attackerDamage = attacker.Damage;
			float defenderDamage = Damage*(1+Location.Province.Land.Terrain.DefenderAdvantage);
			attacker.TakeDamage(defenderDamage, KillRate);
			if (attacker.CurrentManpower <= 0){
				EndBattle(this, attacker);
				return BattleResult.DefenderWon;
			}
			TakeDamage(attackerDamage, attacker.KillRate);
			if (CurrentManpower <= 0){
				EndBattle(attacker, this);
				return BattleResult.AttackerWon;
			}
			return BattleResult.Ongoing;
		}
		private float Damage => AttackPower*RandomDamageMultiplier*CurrentManpower;
		private void TakeDamage(float damage, float killRate){
			int previousManpower = CurrentManpower;
			CurrentManpower = Mathf.Max(CurrentManpower-(int)(damage/Toughness), 0);
			DemoralizedManpower += (int)((previousManpower-CurrentManpower)*(1-killRate));
		}

		private static void EndBattle(Regiment winner, Regiment loser){
			loser.StackWipe();
			winner.CurrentManpower += winner.DemoralizedManpower;
			winner.DemoralizedManpower = 0;
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