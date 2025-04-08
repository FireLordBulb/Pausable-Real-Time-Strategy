using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Regiment> {
		[SerializeField] private float monthlyReinforcementRate;
		[SerializeField] private float orderedRetreatDamageMultiplier;
		[SerializeField] private float stackWipeThreshold;
		
		private int maxMonthlyReinforcement;
		
		public float AttackPower {get; private set;}
		public float Toughness {get; private set;}
		public float KillRate {get; private set;}
		public int MaxManpower {get; private set;}
		public int CurrentManpower {get; private set;}
		public int DemoralizedManpower {get; private set;}
		
		private float Damage => IsMoving ? AttackPower*CurrentManpower*orderedRetreatDamageMultiplier : AttackPower*CurrentManpower;
		public override string CreatingVerb => "Recruiting";
		
		internal void Init(float attackPower, float toughness, float killRate, int manpower){
			AttackPower = attackPower;
			Toughness = toughness;
			KillRate = killRate;
			CurrentManpower = MaxManpower = manpower;
			DemoralizedManpower = 0;
			maxMonthlyReinforcement = (int)(MaxManpower*monthlyReinforcementRate);
			Calendar.Instance.OnMonthTick.AddListener(Reinforce);
		}
		
		protected override Location<Regiment> GetLocation(ProvinceLink link){
			return link.Target.Land.ArmyLocation;
		}
		protected override (Location<Regiment>, int) CalculatePathLocation(){
			ProvinceLink link = PathToTarget[PathIndex];
			return (GetLocation(link), GetTravelDays(link));
		}
		private int GetTravelDays(ProvinceLink link){
			float terrainSpeedMultiplier = 1+0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier);
			return Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier));
		}
		protected override bool LinkEvaluator(ProvinceLink link){
			return link is LandLink landLink && (IsRetreating || Owner == landLink.TargetLand.Owner || Owner.GetDiplomaticStatus(landLink.TargetLand.Owner).IsAtWar);
		}
		
		private void Reinforce(){
			if (CurrentManpower == MaxManpower){
				return;
			}
			if (IsUnsafe(Location) || IsMoving && IsUnsafe(NextLocation)){
				return;
			}
			int reinforcementAmount = Mathf.Min(maxMonthlyReinforcement, Owner.Manpower, MaxManpower-CurrentManpower);
			Owner.GainResources(0, -reinforcementAmount, 0);
			CurrentManpower += reinforcementAmount;
		}
		private bool IsUnsafe(Location<Regiment> location){
			if (location.IsBattleOngoing){
				return false;
			}
			Land nextProvince = location.Province.Land;
			return nextProvince.Owner != Owner || nextProvince.IsOccupied;
		}
		
		internal override BattleResult DoBattle(List<Regiment> defenders, List<Regiment> attackers){
			return DoBattle(defenders, attackers, Location.Province.Land.Terrain);
		}
		private static BattleResult DoBattle(List<Regiment> defenders, List<Regiment> attackers, Terrain terrain){
			(List<Regiment> frontLineDefenders, float defenderProportionOnFrontLine) = GetFrontLine(defenders, terrain.CombatWidth);
			(List<Regiment> frontLineAttackers, float attackerProportionOnFrontLine) = GetFrontLine(attackers, terrain.CombatWidth);
			(float defenderDamage, float defenderKillRate) = CalculateDamage(frontLineDefenders, defenderProportionOnFrontLine*(1+terrain.DefenderAdvantage));
			(float attackerDamage, float attackerKillRate) = CalculateDamage(frontLineAttackers, attackerProportionOnFrontLine);
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
		private static (List<Regiment>, float) GetFrontLine(List<Regiment> army, int combatWidth){
			List<Regiment> frontLine = new();
			int manpowerSoFar = 0;
			foreach (Regiment regiment in army){
				if (0 >= regiment.CurrentManpower){
					continue;
				}
				frontLine.Add(regiment);
				manpowerSoFar += regiment.CurrentManpower;
				if (combatWidth <= manpowerSoFar){
					break;
				}
			}
			// If combat width is say 1000, then maybe the List frontLine will include 3 regiments of 400 men each, for a total of 1200.
			// Even though all 3 regiments are on the front line, only 1000 out of the 1200 men in those regiments is actively fighting.
			// That's why this proportion value is needed to scale down the damage to represent that not every single soldier is the
			// frontLine regiments is personally actively attacking. 
			float proportionOnFrontLine = manpowerSoFar <= combatWidth ? 1 : combatWidth/(float)manpowerSoFar;
			return (frontLine, proportionOnFrontLine);
		}
		private static (float, float) CalculateDamage(List<Regiment> dealer, float multiplier){
			float totalDamage = dealer.Sum(regiment => regiment.Damage);
			float killRate = dealer.Sum(regiment => regiment.KillRate*regiment.Damage/totalDamage);
			totalDamage *= multiplier;
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
		internal override void OnBattleEnd(bool didWin){
			CurrentManpower += DemoralizedManpower;
			DemoralizedManpower = 0;
			if (didWin){
				return;
			}
			if (stackWipeThreshold < CurrentManpower/(float)MaxManpower){
				RetreatHome();
			} else {
				StackWipe();
			}
		}
		internal void RetreatHome(){
			// If the battle was lost while a manual retreat was in progress, continue with it.
			if (IsMoving){
				return;
			}
			IsRetreating = true;
			int shortestPathLength = int.MaxValue;
			List<ProvinceLink> shortestPath = null;
			foreach (Land province in Owner.ControlledLand){
				if (Location.Province.Land == province){
					continue;
				}
				List<ProvinceLink> path = GetPathTo(province.ArmyLocation);
				if (path == null){
					continue;
				}
				int pathLength = path.Sum(GetTravelDays);
				if (shortestPathLength < pathLength){
					continue;
				}
				shortestPathLength = pathLength;
				shortestPath = path;
			}
			// If the entire owner country is on a different landmass, retreat to a random bordering province instead.
			if (shortestPath == null){
				SetDestination(Location.Province.Links.ElementAt(Random.Range(0, Location.Province.Links.Count())).Target.Land.ArmyLocation);		
				return;
			}
			SetDestination(shortestPath[^1].Target.Land.ArmyLocation);
		}
		internal override void StackWipe(){
			Owner.RemoveRegiment(this);
			Location.Remove(this);
			Destroy(gameObject);
		}
	}
}