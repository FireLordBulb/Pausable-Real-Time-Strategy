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
		
		// Armies of different countries in the same LandLocation are always hostile regardless of the actual DiplomaticStatus between them.
		protected override bool AreHostile(Country defender, Country attacker){
			return true;
		}
		protected override void SpecificStartupLogic(){
			// If you control the land you will count as the defender regardless of who actually moved in the province last.
			if (AttackingUnits[0].Owner == Land.Controller){
				(DefendingUnits, AttackingUnits) = (AttackingUnits, DefendingUnits);
			}
		}
		
		protected override Location<Regiment> HandleLocation(Location<Regiment> location){
			if (location is TransportDeck deck){
				return ((Transport)deck.Transport.Location.Units.First(ship => ship.Owner == deck.Transport.Owner && ship is Transport)).Deck;
			}
			
			return base.HandleLocation(location);
		}
		internal override void Refresh(){
			base.Refresh();
			if (SiegeIsOngoing){
				if (Units.All(regiment => regiment.Owner == Land.Controller)){
					EndSiege();
				} else if (Units.Any(regiment => regiment.Owner == Sieger)){
					if (CannotBeBesiegedBy(Sieger)){
						EndSiege();
					} else {
						SiegeIsPausedBecauseMovement = Units.All(regiment => regiment.IsMoving || regiment.Owner != Sieger);
					}
				} else {
					SiegeIsPausedBecauseMovement = false;
					foreach (Regiment regiment in Units){
						if (CannotBeBesiegedBy(regiment)){
							continue;
						}
						Sieger = regiment.Owner;
						return;
					}
					EndSiege();
				}
			} else {
				foreach (Regiment regiment in Units){
					if (CannotBeBesiegedBy(regiment)){
						continue;
					}
					Sieger = regiment.Owner;
					SiegeDaysLeft = Land.SiegeDays;
					SiegeIsPausedBecauseMovement = false;
					SiegeIsOngoing = true;
					Province.Calendar.OnDayTick.AddListener(TickSiege);
					break;
				}
			}
		}
		private bool CannotBeBesiegedBy(Regiment regiment){
			return regiment.IsMoving || CannotBeBesiegedBy(regiment.Owner);
		}
		private bool CannotBeBesiegedBy(Country country){
			return country == Land.Controller || (country != Land.Owner && !country.GetDiplomaticStatus(Land.Owner).IsAtWar);
		}

		private void TickSiege(){
			if (IsBattleOngoing || SiegeIsPausedBecauseMovement || !SiegeIsOngoing){
				return;
			}
			SiegeDaysLeft--;
			if (SiegeDaysLeft > 0){
				return;
			}
			EndSiege();
			Land.Controller.SiegeEnded.Invoke(Land);
			Land.MakeOccupiedBy(Sieger);
			Land.Controller.SiegeEnded.Invoke(Land);
		}

		private void EndSiege(){
			SiegeIsOngoing = false;
			Province.Calendar.OnDayTick.RemoveListener(TickSiege);
		}
	}
}
