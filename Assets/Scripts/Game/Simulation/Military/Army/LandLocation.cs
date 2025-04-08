using System.Linq;
using UnityEngine;

namespace Simulation.Military {
	public class LandLocation : Location<Regiment> {
		public readonly Land Land;
		
		public Country Sieger {get; private set;}
		public int SiegeDaysLeft {get; private set;}
		public bool SiegeIsOngoing {get; private set;}
		public bool SiegeIsPausedBecauseMovement {get; private set;}
		
		public override string Name => Province.Name;
		public override Province Province => Land.Province;
		public override Vector3 WorldPosition => Land.transform.position;
		
		public LandLocation(Land land){
			Land = land;
		}
		
		protected override void SpecificStartupLogic(){
			// If you control the land you will count as the defender regardless of who actually moved in the province last.
			if (AttackingUnits[0].Owner == Land.Controller){
				(DefendingUnits, AttackingUnits) = (AttackingUnits, DefendingUnits);
			}
		}
		
		internal override void UpdateListeners(){
			// New sieges aren't started during battles, and existing ones are paused.
			if (IsBattleOngoing){
				return;
			}
			if (SiegeIsOngoing){
				if (Units.All(regiment => regiment.Owner == Land.Controller)){
					EndSiege();
				} else if (Units.Any(regiment => regiment.Owner == Sieger)){
					SiegeIsPausedBecauseMovement = Units.All(regiment => regiment.IsMoving || regiment.Owner != Sieger);
				} else {
					SiegeIsPausedBecauseMovement = false;
					foreach (Regiment regiment in Units){
						if (regiment.IsMoving){
							continue;
						}
						Sieger = regiment.Owner;
						return;
					}
					EndSiege();
				}
			} else {
				foreach (Regiment regiment in Units){
					if (regiment.Owner == Land.Controller || regiment.IsMoving){
						continue;
					}
					Sieger = regiment.Owner;
					SiegeDaysLeft = Land.SiegeDays;
					SiegeIsPausedBecauseMovement = false;
					SiegeIsOngoing = true;
					Calendar.Instance.OnDayTick.AddListener(TickSiege);
					break;
				}
			}
		}

		private void TickSiege(){
			if (IsBattleOngoing){
				return;
			}
			// Pause the siege if all sieging regiments are moving.
			if (SiegeIsPausedBecauseMovement){
				return;
			}
			SiegeDaysLeft--;
			if (SiegeDaysLeft > 0){
				return;
			}
			EndSiege();
			Land.MakeOccupiedBy(Sieger);
		}

		private void EndSiege(){
			SiegeIsOngoing = false;
			Calendar.Instance.OnDayTick.RemoveListener(TickSiege);
		}
	}
}
