using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simulation.Military {
	public abstract class Location<T> where T : Branch {
		private readonly List<Unit<T>> units = new();

		private bool battleIsOngoing;
		// TODO: Allow for multiple units on each side.
		private Unit<T> defendingUnit;
		private Unit<T> attackingUnit;
		
		public abstract string Name {get;}
		public virtual Province SearchTargetProvince => Province;
		public abstract Province Province {get;}
		public abstract Vector3 WorldPosition {get;}

		public IEnumerable<Unit<T>> Units => units;
		public bool IsBattleOngoing => defendingUnit != null;
		
		public void Add(Unit<T> unit){
			if (battleIsOngoing){
				throw new NotImplementedException("Reinforcing battles hasn't been implemented yet!");
			}
			if (0 < units.Count){
				Unit<T> alreadyPresentUnit = units[0];
				if (alreadyPresentUnit.Owner != unit.Owner){
					defendingUnit = alreadyPresentUnit;
					attackingUnit = unit;
					Calendar.Instance.OnDayTick.AddListener(BattleTick);
				}
			}
			units.Add(unit);
		}
		public void Remove(Unit<T> unit){
			units.Remove(unit);
			if (battleIsOngoing){
				EndBattle();
			}
		}
		
		private void BattleTick(){
			BattleResult result = DefendBattle();
			if (result == BattleResult.Ongoing){
				return;
			}
			(result == BattleResult.AttackerWon ? defendingUnit : attackingUnit).StackWipe();
			EndBattle();
		}
		private BattleResult DefendBattle(){
			int diceRoll = Random.Range(0, 6);
			if (diceRoll < 3){
				return BattleResult.Ongoing;
			}
			if (diceRoll == 3){
				return BattleResult.AttackerWon;
			}
			return BattleResult.DefenderWon;
		}

		private void EndBattle(){
			defendingUnit = null;
			attackingUnit = null;
			Calendar.Instance.OnDayTick.RemoveListener(BattleTick);
		}
		
		public virtual void AdjustPath(List<ProvinceLink> path){}
		public override string ToString(){
			return Name;
		}
	}
}
