using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Location<TUnit> where TUnit : Unit<TUnit> {
		private readonly List<TUnit> units = new();
		
		protected List<TUnit> DefendingUnits;
		protected List<TUnit> AttackingUnits;
		protected TUnit CommandingDefendingUnit;
		protected TUnit CommandingAttackingUnit;
		
		public abstract string Name {get;}
		public virtual Province SearchTargetProvince => Province;
		public abstract Province Province {get;}
		public abstract Vector3 WorldPosition {get;}
		public bool IsBattleOngoing {get; private set;}
		
		public IEnumerable<TUnit> Units => units;
		
		public void Add(TUnit unit){
			if (unit.IsRetreating){
				// Pass by the battle, nothing to see here.
			} else if (IsBattleOngoing){
				if (unit.Owner == CommandingDefendingUnit.Owner){
					unit.StopMoving();
					DefendingUnits.Add(unit);
				} else if (unit.Owner == CommandingAttackingUnit.Owner){
					unit.StopMoving();
					AttackingUnits.Add(unit);
				} else {
					// Third parties can't join ongoing battles, and may pass through provinces where others are battling undisturbed.
				}
			} else if (0 < units.Count){
				PossiblyStartBattle(unit);
			}
			units.Add(unit);
			UpdateListeners();
		}

		private void PossiblyStartBattle(TUnit unit){
			TUnit firstUnit = units[0];
			if (firstUnit.Owner == unit.Owner){
				return;
			}
			DefendingUnits = new List<TUnit>();
			foreach (TUnit presentUnit in units){
				if (!presentUnit.IsRetreating){
					DefendingUnits.Add(presentUnit);
				}
			}
			AttackingUnits = new List<TUnit>{unit};
			StartBattle();
			for (int i = units.Count-1; i >= 0; i--){
				TUnit defendingUnit = units[i];
				if (!defendingUnit.IsBuilt){
					defendingUnit.StackWipe();
				}
			}
		}
		public void Remove(TUnit unit){
			units.Remove(unit);
			RemoveFromSide(unit, DefendingUnits, BattleResult.AttackerWon);
			RemoveFromSide(unit, AttackingUnits, BattleResult.DefenderWon);
			// Assign a new commanding unit if the commander left the battle.
			if (IsBattleOngoing){
				if (CommandingDefendingUnit == unit){
					CommandingDefendingUnit = DefendingUnits[0];
				} else if (CommandingAttackingUnit == unit){
					CommandingAttackingUnit = AttackingUnits[0];
				}
			}
			UpdateListeners();
		}
		private void RemoveFromSide(TUnit unit, List<TUnit> side, BattleResult result){
			if (!IsBattleOngoing || side.Count == 0 || unit.Owner != side[0].Owner){
				return;
			}
			side.Remove(unit);
			if (side.Count == 0){
				EndBattle(result);
			}
		}
		
		private void BattleTick(){
			if (!IsBattleOngoing){
				return;
			}
			CommandingDefendingUnit.CommanderBattleTick();
			CommandingAttackingUnit.CommanderBattleTick();
			BattleResult result = CommandingDefendingUnit.DoBattle(DefendingUnits, AttackingUnits);
			if (result != BattleResult.Ongoing){
				EndBattle(result);
			}
		}

		private void EndBattle(BattleResult result){
			if (!IsBattleOngoing){
				return;
			}
			Province.Calendar.OnDayTick.RemoveListener(BattleTick);
			IsBattleOngoing = false;
			bool didDefenderWin = result == BattleResult.DefenderWon;
			CommandingDefendingUnit.CommanderOnBattleEnd(didDefenderWin, this);
			foreach (TUnit unit in DefendingUnits){
				unit.OnBattleEnd(didDefenderWin);
			}
			bool didAttackerWin = result == BattleResult.AttackerWon;
			CommandingAttackingUnit.CommanderOnBattleEnd(didAttackerWin, this);
			foreach (TUnit unit in AttackingUnits){
				unit.OnBattleEnd(didAttackerWin);
			}
			Country winningCountry = didDefenderWin ? CommandingDefendingUnit.Owner : CommandingAttackingUnit.Owner;
			CommandingDefendingUnit = CommandingAttackingUnit = null;
			DefendingUnits = AttackingUnits = null;
			UpdateListeners();
			BattleWithThirdParty(winningCountry);
		}

		private void BattleWithThirdParty(Country winningCountry){
			// Instantly start another battle if there is a third party present in the province.
			DefendingUnits = new List<TUnit>();
			AttackingUnits = new List<TUnit>();
			Country thirdParty = null;
			foreach (TUnit unit in units){
				if (unit.IsRetreating){
					continue;
				}
				if (unit.Owner == winningCountry){
					DefendingUnits.Add(unit);
				} else if (unit.Owner == thirdParty){
					AttackingUnits.Add(unit);
				} else if (thirdParty == null){
					thirdParty = unit.Owner;
					AttackingUnits.Add(unit);
				}
			}
			if (thirdParty != null){
				StartBattle();
			}
		}
		
		private void StartBattle(){
			SpecificStartupLogic();
			CommandingDefendingUnit = DefendingUnits[0];
			CommandingAttackingUnit = AttackingUnits[0];
			CommandingDefendingUnit.CommanderBattleStartUp();
			CommandingAttackingUnit.CommanderBattleStartUp();
			foreach (TUnit unit in DefendingUnits){
				unit.StopMoving();
			}
			foreach (TUnit unit in AttackingUnits){
				unit.StopMoving();
			}
			IsBattleOngoing = true;
			Province.Calendar.OnDayTick.AddListener(BattleTick);
		}
		protected virtual void SpecificStartupLogic(){}
		internal virtual void UpdateListeners(){}
		
		public virtual void AdjustPath(List<ProvinceLink> path){}
		public override string ToString(){
			return Name;
		}
	}
}
