using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Regiment> {
		public float AttackPower {get; private set;}
		public float Toughness {get; private set;}
		public float KillRate {get; private set;}
		public int MaxManpower {get; private set;}
		public int CurrentManpower {get; private set;}
		public int DemoralizedManpower {get; private set;}
		
		private float Damage => AttackPower*CurrentManpower;
		
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
		
		public override BattleResult DoBattle(List<Regiment> defenders, List<Regiment> attackers){
			return DoBattle(defenders, attackers, Location.Province.Land.Terrain);
		}
		private static BattleResult DoBattle(List<Regiment> defenders, List<Regiment> attackers, Terrain terrain){
			List<Regiment> frontLineDefenders = GetFrontLine(defenders);
			List<Regiment> frontLineAttackers = GetFrontLine(attackers);
			(float defenderDamage, float defenderKillRate) = CalculateDamage(frontLineDefenders, terrain.DefenderAdvantage);
			(float attackerDamage, float attackerKillRate) = CalculateDamage(frontLineAttackers);
			ApplyDamage(frontLineAttackers, defenderDamage, defenderKillRate);
			if (attackers.Sum(attacker => attacker.CurrentManpower) <= 0){
				return BattleResult.DefenderWon;
			}
			ApplyDamage(frontLineDefenders, attackerDamage, attackerKillRate);
			if (defenders.Sum(defender => defender.CurrentManpower) <= 0){
				return BattleResult.AttackerWon;
			}
			return BattleResult.Ongoing;
		}
		private static List<Regiment> GetFrontLine(List<Regiment> army){
			List<Regiment> frontLine = new();
			foreach (Regiment regiment in army){
				if (0 < regiment.CurrentManpower){
					frontLine.Add(regiment);
				}
			}
			return frontLine;
		}
		private static (float, float) CalculateDamage(List<Regiment> dealer, float bonusAdvantage = 0f){
			float totalDamage = dealer.Sum(regiment => regiment.Damage);
			float killRate = dealer.Sum(regiment => regiment.KillRate*regiment.Damage/totalDamage);
			totalDamage *= 1+bonusAdvantage;
			// TODO: Make the multiplier being the same for the whole side less awkward.
			totalDamage *= dealer[0].RandomDamageMultiplier;
			return (totalDamage, killRate);
		}
		private static void ApplyDamage(List<Regiment> taker, float totalDamage, float killRate){
			int frontLineManpower = taker.Sum(regiment => regiment.CurrentManpower);
			float averageToughness = taker.Sum(regiment => regiment.Toughness*regiment.CurrentManpower/frontLineManpower);
			int maxManpowerLoss = (int)(totalDamage/averageToughness);
			if (maxManpowerLoss >= frontLineManpower){
				float survivalRate = Mathf.Max(frontLineManpower-maxManpowerLoss*killRate, 0)/frontLineManpower;
				foreach (Regiment regiment in taker){
					regiment.DemoralizedManpower += (int)(regiment.CurrentManpower*survivalRate*regiment.Toughness/averageToughness);
					regiment.CurrentManpower = 0;
				}
			} else {
				float demoralizedRate = 1-killRate;
				foreach (Regiment regiment in taker){
					int manpowerLoss = (int)(regiment.CurrentManpower*(maxManpowerLoss*regiment.Toughness)/(frontLineManpower*averageToughness));
					regiment.DemoralizedManpower += (int)(manpowerLoss*demoralizedRate);
					regiment.CurrentManpower -= manpowerLoss;
				}
			}
		}
		public override void OnBattleEnd(bool didWin){
			if (didWin){
				CurrentManpower += DemoralizedManpower;
				DemoralizedManpower = 0;
			} else {
				StackWipe();
			}
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