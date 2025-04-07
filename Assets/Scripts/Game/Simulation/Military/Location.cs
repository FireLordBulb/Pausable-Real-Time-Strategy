using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Location<TUnit> where TUnit : Unit<TUnit> {
		private readonly List<TUnit> units = new();

		// TODO: Allow for multiple units on each side.
		private List<TUnit> defendingUnits;
		private List<TUnit> attackingUnits;
		
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
				if (unit.Owner == defendingUnits[0].Owner){
					unit.StopMoving();
					defendingUnits.Add(unit);
				} else if (unit.Owner == attackingUnits[0].Owner){
					unit.StopMoving();
					attackingUnits.Add(unit);
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
			defendingUnits = new List<TUnit>();
			foreach (TUnit presentUnit in units){
				if (!presentUnit.IsRetreating){
					defendingUnits.Add(presentUnit);
				}
			}
			attackingUnits = new List<TUnit>{unit};
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
			RemoveFromSide(unit, defendingUnits, BattleResult.AttackerWon);
			RemoveFromSide(unit, attackingUnits, BattleResult.DefenderWon);
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
			defendingUnits[0].RerollBattleRandomness();
			attackingUnits[0].RerollBattleRandomness();
			BattleResult result = defendingUnits[0].DoBattle(defendingUnits, attackingUnits);
			if (result != BattleResult.Ongoing){
				EndBattle(result);
			}
		}

		private void EndBattle(BattleResult result){
			if (!IsBattleOngoing){
				return;
			}
			Calendar.Instance.OnDayTick.RemoveListener(BattleTick);
			IsBattleOngoing = false;
			bool didDefenderWin = result == BattleResult.DefenderWon;
			foreach (TUnit unit in defendingUnits){
				unit.OnBattleEnd(didDefenderWin);
			}
			bool didAttackerWin = result == BattleResult.AttackerWon;
			foreach (TUnit unit in attackingUnits){
				unit.OnBattleEnd(didAttackerWin);
			}
			Country winningCountry = didDefenderWin ? defendingUnits[0].Owner : attackingUnits[0].Owner;
			defendingUnits = attackingUnits = null;
			UpdateListeners();
			BattleWithThirdParty(winningCountry);
		}

		private void BattleWithThirdParty(Country winningCountry){
			// Instantly start another battle if there is a third party present in the province.
			defendingUnits = new List<TUnit>();
			attackingUnits = new List<TUnit>();
			Country thirdParty = null;
			foreach (TUnit unit in units){
				if (unit.IsRetreating){
					continue;
				}
				if (unit.Owner == winningCountry){
					defendingUnits.Add(unit);
				} else if (unit.Owner == thirdParty){
					attackingUnits.Add(unit);
				} else if (thirdParty == null){
					thirdParty = unit.Owner;
					attackingUnits.Add(unit);
				}
			}
			if (thirdParty != null){
				StartBattle();
			}
		}
		
		private void StartBattle(){
			defendingUnits[0].StartupBattleRandomness();
			attackingUnits[0].StartupBattleRandomness();
			foreach (TUnit unit in defendingUnits){
				unit.StopMoving();
			}
			foreach (TUnit unit in attackingUnits){
				unit.StopMoving();
			}
			IsBattleOngoing = true;
			Calendar.Instance.OnDayTick.AddListener(BattleTick);
		}

		internal virtual void UpdateListeners(){}
		
		public virtual void AdjustPath(List<ProvinceLink> path){}
		public override string ToString(){
			return Name;
		}
	}
}
