using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simulation.Military {
	public abstract class Location<TUnit> where TUnit : Unit<TUnit> {
		private readonly List<TUnit> units = new();

		private bool battleIsOngoing;
		// TODO: Allow for multiple units on each side.
		private TUnit defendingUnit;
		private TUnit attackingUnit;
		
		public abstract string Name {get;}
		public virtual Province SearchTargetProvince => Province;
		public abstract Province Province {get;}
		public abstract Vector3 WorldPosition {get;}

		public IEnumerable<TUnit> Units => units;
		public bool IsBattleOngoing => defendingUnit != null;
		
		public void Add(TUnit unit){
			if (battleIsOngoing){
				throw new NotImplementedException("Reinforcing battles hasn't been implemented yet!");
			}
			if (0 < units.Count){
				TUnit alreadyPresentUnit = units[0];
				if (alreadyPresentUnit.Owner != unit.Owner){
					defendingUnit = alreadyPresentUnit;
					attackingUnit = unit;
					defendingUnit.StartBattle();
					attackingUnit.StartBattle();
					Calendar.Instance.OnDayTick.AddListener(BattleTick);
				}
			}
			units.Add(unit);
		}
		public void Remove(TUnit unit){
			units.Remove(unit);
			if (battleIsOngoing){
				EndBattle();
			}
		}
		
		private void BattleTick(){
			defendingUnit.RerollBattleRandomness();
			attackingUnit.RerollBattleRandomness();
			BattleResult result = defendingUnit.DefendBattle(attackingUnit);
			if (result != BattleResult.Ongoing){
				EndBattle();
			}
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
