using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Location<TUnit> : ILocation where TUnit : Unit<TUnit> {
		private readonly List<TUnit> units = new();
		
		protected List<TUnit> DefendingUnits;
		protected List<TUnit> AttackingUnits;
		
		public abstract string Name {get;}
		internal virtual Province SearchProvince => Province;
		public abstract Province Province {get;}
		public abstract Vector3 WorldPosition {get;}
		public bool IsBattleOngoing {get; private set;}
		internal TUnit CommandingDefendingUnit {get; private set;}
		internal TUnit CommandingAttackingUnit {get; private set;}
		
		public IReadOnlyList<TUnit> Units => units;
		public Country DefendingCountry => CommandingDefendingUnit == null ? null : CommandingDefendingUnit.Owner;
		public Country AttackingCountry => CommandingAttackingUnit == null ? null : CommandingAttackingUnit.Owner;
		
		public void Add(TUnit unit){
			if (unit.IsRetreating){
				// Pass by the battle, nothing to see here.
			} else if (IsBattleOngoing){
				if (unit.Owner == CommandingDefendingUnit.Owner){
					unit.OnBattleStart(true);
					DefendingUnits.Add(unit);
				} else if (unit.Owner == CommandingAttackingUnit.Owner){
					unit.OnBattleStart(false);
					AttackingUnits.Add(unit);
				} else {
					// Third parties can't join ongoing battles, and may pass through provinces where others are battling undisturbed.
				}
			} else if (0 < units.Count){
				PossiblyStartBattle(unit);
			}
			units.Add(unit);
			Refresh();
		}
		private void PossiblyStartBattle(TUnit unit){
			TUnit firstUnit = units[0];
			if (firstUnit.Owner == unit.Owner || !AreHostile(firstUnit.Owner, unit.Owner)){
				return;
			}
			DefendingUnits = new List<TUnit>();
			foreach (TUnit presentUnit in units){
				if (!presentUnit.IsRetreating && presentUnit.Owner == firstUnit.Owner){
					DefendingUnits.Add(presentUnit);
				}
			}
			if (DefendingUnits.Count == 0){
				return;
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
			Refresh();
		}
		private void RemoveFromSide(TUnit unit, List<TUnit> side, BattleResult result){
			if (!IsBattleOngoing || side.Count == 0 || unit.Owner != side[0].Owner){
				return;
			}
			side.Remove(unit);
			if (side.Count == 0){
				EndBattle(result);
			} else {
				unit.BattleEnd(true);
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
				unit.BattleEnd(didDefenderWin);
			}
			bool didAttackerWin = result == BattleResult.AttackerWon;
			CommandingAttackingUnit.CommanderOnBattleEnd(didAttackerWin, this);
			foreach (TUnit unit in AttackingUnits){
				unit.BattleEnd(didAttackerWin);
			}
			Country winningCountry = didDefenderWin ? CommandingDefendingUnit.Owner : CommandingAttackingUnit.Owner;
			CommandingDefendingUnit = CommandingAttackingUnit = null;
			DefendingUnits = AttackingUnits = null;
			Refresh();
			BattleWithThirdParty(winningCountry);
		}

		internal void RecheckIfBattleShouldStart(){
			if (!IsBattleOngoing && units.Count > 0){
				BattleWithThirdParty(units[0].Owner);
			}
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
			if (AttackingUnits.Count > 0 && DefendingUnits.Count > 0 && AreHostile(DefendingUnits[0].Owner, AttackingUnits[0].Owner)){
				StartBattle();
			}
		}
		protected abstract bool AreHostile(Country defender, Country attacker);
		
		private void StartBattle(){
			SpecificStartupLogic();
			CommandingDefendingUnit = DefendingUnits[0];
			CommandingAttackingUnit = AttackingUnits[0];
			CommandingDefendingUnit.CommanderBattleStartUp();
			CommandingAttackingUnit.CommanderBattleStartUp();
			foreach (TUnit unit in DefendingUnits){
				unit.OnBattleStart(true);
			}
			foreach (TUnit unit in AttackingUnits){
				unit.OnBattleStart(false);
			}
			IsBattleOngoing = true;
			Province.Calendar.OnDayTick.AddListener(BattleTick);
		}
		protected virtual void SpecificStartupLogic(){}
		internal virtual void Refresh(){
			Refresh((_, _) => {});
		}
		internal void Refresh(Action<TUnit, (Location<TUnit>, BattleSide)> extraAction){
			Dictionary<(Location<TUnit>, BattleSide), int> sharedIndices = new();
			Country defendingCountry = null, attackingCountry = null;
			if (IsBattleOngoing){
				defendingCountry = CommandingDefendingUnit.Owner;
				attackingCountry = CommandingAttackingUnit.Owner;
			}
			foreach (TUnit unit in Units){
				(Location<TUnit> nextLocation, BattleSide side) key = (HandleLocation(unit.NextLocation), BattleSide.None);
				if (unit.Owner == defendingCountry){
					key.side = BattleSide.Defending;
				} else if (unit.Owner == attackingCountry){
					key.side = BattleSide.Attacking;
				}
				if (!sharedIndices.TryGetValue(key, out int index)){
					index = 0;
				}
				unit.SetSharedPositionIndex(index);
				sharedIndices[key] = index+1;
				extraAction(unit, key);
			}
		}
		protected virtual Location<TUnit> HandleLocation(Location<TUnit> location){
			return location;
		}
		
		public virtual void AdjustPathStart(List<ProvinceLink> path){}
		public virtual void AdjustPathEnd(List<ProvinceLink> path){}
		public override string ToString(){
			return Name;
		}
	}
	public enum BattleSide {
		None,
		Defending,
		Attacking
	}
	public interface ILocation {
		public string Name {get;}
		public Province Province {get;}
		public Vector3 WorldPosition {get;}
		public bool IsBattleOngoing {get;}
	}
}
