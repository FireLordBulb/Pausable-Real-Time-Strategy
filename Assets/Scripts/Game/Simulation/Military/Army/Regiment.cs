using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Regiment> {
		[Header("Regiment-specific Values")]
		[SerializeField] private float monthlyReinforcementRate;
		[SerializeField] private float orderedRetreatDamageMultiplier;
		[SerializeField] private float stackWipeThreshold;
		[Header("Soldier")]
		[SerializeField] private GameObject soldierModel;
		[Header("Flag")]
		[SerializeField] private Transform flag;
		[SerializeField] private float flagHeightStep;
		
		private float maintenance;
		private int maxMonthlyReinforcement;
		private float defaultFlagHeight;
		
		public float AttackPower {get; private set;}
		public float Toughness {get; private set;}
		public float KillRate {get; private set;}
		public int MaxManpower {get; private set;}
		public int CurrentManpower {get; private set;}
		public int DemoralizedManpower {get; private set;}
		
		private float Damage => IsMoving ? AttackPower*CurrentManpower*orderedRetreatDamageMultiplier : AttackPower*CurrentManpower;
		public override string CreatingVerb => "Recruiting";
		public override string HpText => $"{ManpowerText} Manpower";
		public string ManpowerText => $"{CurrentManpower}{(DemoralizedManpower == 0 ? "" : $"({CurrentManpower+DemoralizedManpower})")}/{MaxManpower}";
		
		internal void Init(float attackPower, float toughness, float killRate, float maintenanceCost, int manpower){
			AttackPower = attackPower;
			Toughness = toughness;
			KillRate = killRate;
			CurrentManpower = MaxManpower = manpower;
			DemoralizedManpower = 0;
			maintenance = maintenanceCost;
			maxMonthlyReinforcement = (int)(MaxManpower*monthlyReinforcementRate);
			defaultFlagHeight = flag.localPosition.y;
			Province.Calendar.OnMonthTick.AddListener(PayMaintenance);
		}
		protected override void VisualizeSharedPositionIndex(int index){
			// Only render one soldier model per shared position.
			soldierModel.SetActive(index == 0);
			Vector3 localPosition = flag.localPosition;
			localPosition.y = defaultFlagHeight + flagHeightStep*index;
			flag.localPosition = localPosition;
		}
		internal override void OnFinishBuilding(){
			Owner.RegimentBuilt.Invoke(this);
		}
		
		protected override Location<Regiment> GetLocation(ProvinceLink link){
			return GetLocation(link, TargetLocation, true);
		}
		// Different transports in the same harbor are connected by the same link, so which transport is the target needs to be specified.
		public Location<Regiment> GetLocation(ProvinceLink link, Location<Regiment> targetLocation, bool doAllowDifferentTransport){
			if (link is ShallowsLink shallowsLink){
				if (!IsLocationInHarbor(targetLocation, shallowsLink.Harbor)){
					return null;
				}
				TransportDeck deck = (TransportDeck)targetLocation;
				bool canRegimentBoard = deck.Transport.CanRegimentBoard(this);
				if (canRegimentBoard){
					return deck;
				}
				if (!doAllowDifferentTransport){
					return null;
				}
				foreach (Ship ship in deck.Transport.Location.Units){
					if (ship == deck.Transport || ship is not Transport transport || !transport.CanRegimentBoard(this)){
						continue;
					}
					OverrideTargetLocation();
					return transport.Deck;
				}
				return null;
			}
			if (link is CoastLink coastLink){
				if (!IsLocationInHarbor(Location, coastLink.Harbor)){
					return null;
				}
			}
			return link.Target.Land.ArmyLocation;
		}
		private static bool IsLocationInHarbor(Location<Regiment> location, Harbor harbor){
			return location is TransportDeck deck && !deck.Transport.IsMoving && deck.Transport.Location == harbor;
		}
		protected override int CalculateTravelDays(){
			return GetTravelDays(PathToTarget[PathIndex]);
		}
		private int GetTravelDays(ProvinceLink link){
			return GetTravelDays(link, MovementSpeed);
		}
		public static int GetTravelDays(ProvinceLink link, float movementSpeed){
			float distance;
			float terrainSpeedMultiplier;
			if (link is HarborLink harborLink){
				distance = Vector3.Distance(harborLink.Land.Province.WorldPosition, harborLink.WorldPosition);
				terrainSpeedMultiplier = harborLink.Land.Province.MoveSpeedMultiplier;
			} else {
				distance = link.Distance;
				terrainSpeedMultiplier = 0.5f*(link.Source.MoveSpeedMultiplier+link.Target.MoveSpeedMultiplier);
			}
			return Mathf.Max(Mathf.RoundToInt(distance/(movementSpeed*terrainSpeedMultiplier)), 1);
		}
		protected override bool LinkEvaluator(ProvinceLink link){
			return LinkEvaluator(link, IsRetreating, Owner);
		}
		public static bool LinkEvaluator(ProvinceLink link, bool doIgnoreOwner, Country owner){
			return link is LandLink landLink && (doIgnoreOwner || owner == landLink.TargetLand.Owner || owner.GetDiplomaticStatus(landLink.TargetLand.Owner).IsAtWar);
		}
		
		private void PayMaintenance(){
			if (this == null){
				return;
			}
			Owner.MonthlyGoldChange(-maintenance, $"{Type.name} Upkeep", GetType());
			Reinforce();
		}
		private void Reinforce(){
			if (CurrentManpower == MaxManpower){
				return;
			}
			if (IsUnsafe(Location) || IsMoving && IsUnsafe(NextLocation)){
				return;
			}
			int reinforcementAmount = Mathf.Min(maxMonthlyReinforcement, Owner.Manpower, MaxManpower-CurrentManpower);
			if (reinforcementAmount <= 0){
				return;
			}
			Owner.MonthlyManpowerChange(-reinforcementAmount, $"Reinforcing {Type.name}", GetType());
			CurrentManpower += reinforcementAmount;
		}
		private bool IsUnsafe(Location<Regiment> location){
			if (location.IsBattleOngoing || location is TransportDeck){
				return true;
			}
			Land land = location.Province.Land;
			return land.Owner != Owner || land.IsOccupied;
		}
		
		internal override BattleResult DoBattle(List<Regiment> defenders, List<Regiment> attackers){
			return DoBattle(defenders, attackers, Location.Province);
		}
		private static BattleResult DoBattle(List<Regiment> defenders, List<Regiment> attackers, Province province){
			(List<Regiment> frontLineDefenders, float defenderProportionOnFrontLine) = GetFrontLine(defenders, province.CombatWidth);
			(List<Regiment> frontLineAttackers, float attackerProportionOnFrontLine) = GetFrontLine(attackers, province.CombatWidth);
			(float defenderDamage, float defenderKillRate) = CalculateDamage(frontLineDefenders, defenderProportionOnFrontLine*province.DefenderDamageMultiplier);
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
			if (dealer.Count == 0){
				return (0, 0);
			}
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
					// Manpower must always go down (so the battle eventually ends), so even if damage is so low that manpowerLoss rounds to 0, set it to at minimum 1.
					manpowerLoss = Mathf.Max(manpowerLoss, 1);
					regiment.DemoralizedManpower += (int)(manpowerLoss*demoralizedRate);
					regiment.CurrentManpower -= manpowerLoss;
				}
			}
		}
		internal override void CommanderOnBattleEnd(bool didWin, Location<Regiment> location){
			Owner.LandBattleEnded.Invoke(location);
		}
		protected override void OnBattleEnd(bool didWin){
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
			if (Owner.ProvinceCount == 0){
				return;
			}
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
				Land[] borderingLands = Location.Province.Links.Where(link => link is LandLink).Select(link => link.Target.Land).ToArray();
				if (borderingLands.Length != 0){
					SetDestination(borderingLands[Random.Range(0, borderingLands.Length)].ArmyLocation);
				} else {
					// If there's truly nowhere to retreat, just die.
					StackWipe();
				}
				return;
			}
			SetDestination(shortestPath[^1].Target.Land.ArmyLocation);
		}
		internal override void StackWipe(){
			base.StackWipe();
			Owner.RemoveRegiment(this);
			Province.Calendar.OnMonthTick.RemoveListener(PayMaintenance);
			if (this != null){
				Destroy(gameObject);
			}
		}
	}
}
