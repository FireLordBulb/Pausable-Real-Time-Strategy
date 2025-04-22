using System.Collections.Generic;
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
		
		public override string CreatingVerb => "Constructing";
		
		internal void Init(float attackPower, int hull, float gold, int sailors){
			AttackPower = attackPower;
			IntactHull = MaxHull = hull;
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
			IntactHull += (int)(MaxHull*hullToRepair);
		}
		
		internal override BattleResult DoBattle(List<Ship> defenders, List<Ship> attackers){
			return BattleResult.DefenderWon;
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