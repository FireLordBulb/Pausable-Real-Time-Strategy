using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Military {
	public class Ship : Unit<Ship> {
		[Header("Ship-specific Values")]
		[SerializeField] private float monthlyReparationRate;
		[SerializeField] private float reparationCostFraction;
		[SerializeField] private float orderedRetreatDamageMultiplier;
		[SerializeField] private AnimationCurve sinkingProbability;
		
		private int maxMonthlyReparation;
		private float fullRepairGoldCost;
		private int fullRepairSailorsCost;
		
		public float AttackPower {get; private set;}
		public int MaxHull {get; private set;}
		public int IntactHull {get; private set;}
		public float Size {get; private set;}
		
		public override string CreatingVerb => "Constructing";
		protected virtual bool ShouldAvoidCombat => false;
		
		internal void Init(float attackPower, int hull, float size, float gold, int sailors){
			AttackPower = attackPower;
			IntactHull = MaxHull = hull;
			Size = size;
			maxMonthlyReparation = (int)(MaxHull*monthlyReparationRate);
			fullRepairGoldCost = gold*reparationCostFraction;
			fullRepairSailorsCost = (int)(sailors*reparationCostFraction);
			Province.Calendar.OnMonthTick.AddListener(Repair);
		}
		protected override void OnFinishBuilding(){
			Owner.ShipBuilt.Invoke(this);
		}
		
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
		protected override bool LinkEvaluator(ProvinceLink link){
			return link is SeaLink or CoastLink or ShallowsLink;
		}

		private void Repair(){
			if (IntactHull == MaxHull){
				return;
			}
			if (IsMoving || Location.IsBattleOngoing || Location is not Harbor harbor || harbor.Land.IsOccupied || harbor.Land.Owner != Owner){
				return;
			}
			float hullToRepair = Mathf.Min(maxMonthlyReparation, MaxHull-IntactHull);
			hullToRepair = Mathf.Min(hullToRepair, hullToRepair*Owner.Gold/fullRepairGoldCost, hullToRepair*Owner.Sailors/fullRepairSailorsCost);
			Owner.ChangeResources(-fullRepairGoldCost*hullToRepair/MaxHull, 0, -(int)(fullRepairSailorsCost*hullToRepair/MaxHull));
			IntactHull += (int)(hullToRepair);
		}
		
		internal override BattleResult DoBattle(List<Ship> defenders, List<Ship> attackers){
			return DoBattle(defenders, attackers, Location is Harbor);
		}
		// Always return BattleResult.Ongoing because EndBattle will instead get called when the ships get stackWiped or manually retreat.
		private static BattleResult DoBattle(List<Ship> defenders, List<Ship> attackers, bool isHarbor){
			List<Ship> frontLineDefenders = GetFrontLine(defenders, isHarbor);
			List<Ship> frontLineAttackers = GetFrontLine(attackers, isHarbor);
			float defenderDamage = CalculateDamage(frontLineDefenders);
			float attackerDamage = CalculateDamage(frontLineAttackers);
			ApplyDamage(frontLineAttackers, defenderDamage);
			// If the attackers got stackWiped by the defender attack they may not do any damage.
			if (attackers.Count <= 0){
				return BattleResult.Ongoing;
			}
			ApplyDamage(frontLineDefenders, attackerDamage);
			return BattleResult.Ongoing;
		}
		private static List<Ship> GetFrontLine(List<Ship> navy, bool isHarbor){
			if (isHarbor){
				return new List<Ship>(navy);
			}
			List<Ship> frontLine = navy.Where(ship => !ship.ShouldAvoidCombat).ToList();
			return frontLine.Count == 0 ? new List<Ship>(navy) : frontLine;
		}
		private static float CalculateDamage(List<Ship> dealer){
			float totalDamage = dealer.Sum(ship => ship.IsMoving ? ship.AttackPower*ship.orderedRetreatDamageMultiplier : ship.AttackPower);
			// TODO: Make the multiplier being the same for the whole side less awkward.
			totalDamage *= dealer[0].RandomDamageMultiplier;
			return totalDamage;
		}
		private static void ApplyDamage(List<Ship> taker, float totalDamage){
			float totalTargetSize = taker.Sum(ship => ship.Size);
			foreach (Ship ship in taker){
				ship.IntactHull -= (int)(totalDamage*ship.Size/totalTargetSize);
				if (ship.IntactHull <= 0 || Random.value < ship.sinkingProbability.Evaluate(ship.IntactHull/(float)ship.MaxHull)){
					ship.StackWipe();
				}
			}
		}
		internal override void CommanderOnBattleEnd(bool didWin, Location<Ship> location){
			Owner.SeaBattleEnded.Invoke(location);
		}
		internal override void OnBattleEnd(bool didWin){
			if (!didWin){
				StackWipe();
			}
		}
		internal override void StackWipe(){
			Owner.RemoveShip(this);
			Location.Remove(this);
			Destroy(gameObject);
		}
	}
}