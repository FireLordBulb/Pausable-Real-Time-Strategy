using System.Globalization;
using Simulation;
using Simulation.Military;
using Text;

namespace Player {
	public class RegimentWindow : MilitaryUnitWindow<Regiment> {
		public override void Refresh(){
			base.Refresh();
			if (!Selected.IsBuilt || Selected.IsMoving || Selected.Location.IsBattleOngoing || Selected.Province.IsSea){
				return;
			}
			LandLocation armyLocation = Selected.Province.Land.ArmyLocation;
			if (armyLocation.SiegeIsPausedBecauseMovement || !armyLocation.SiegeIsOngoing){
				return;
			}
			SetLeftOfLinkText("Besieging ");
			days.text = armyLocation.SiegeDaysLeft.ToString();
			daysLeftText.SetActive(true);
		}
		protected override void RefreshCombatTable(){
			combatValuesTable.UpdateColumn(-1, (
				Selected.ManpowerText),
				Selected.AttackPower.ToString("0.#", CultureInfo.InvariantCulture),
				Selected.Toughness.ToString("0.#", CultureInfo.InvariantCulture),
				Format.Percent(Selected.KillRate)
			);
		}
		
		protected override bool DoBypassDefaultBehaviour(ISelectable clickedSelectable){
			if (clickedSelectable is not Transport transport){
				return false;
			}
			TransportDeck deck = transport.Deck;
			MoveOrderResult result = Player.MoveRegimentTo(Selected, deck);
			SetMessageFromResult(result, deck.Name);
			MoveOtherRegimentsTo(deck);
			return true;
		}
		protected override void OrderMove(Province province){
			Location<Regiment> location = province.IsLand ? province.Land.ArmyLocation : null;
			MoveOrderResult result = Player.MoveRegimentTo(Selected, location);
			SetMessageFromResult(result, province.Name);
			MoveOtherRegimentsTo(location);
		}
		private void SetMessageFromResult(MoveOrderResult result, string targetName){
			SetMessage(result switch {
				MoveOrderResult.BusyRetreating => "Cannot interrupt retreat movement!",
				MoveOrderResult.NotBuilt => "Cannot move an army before it has finished recruiting!",
				MoveOrderResult.NoPath => $"Cannot move to {targetName} because it's separated by sea or the land of countries you're at peace with!",
				MoveOrderResult.NoAccess => "You cannot cross another country's borders when you're at peace with it!",
				MoveOrderResult.InvalidTarget => "Armies cannot walk on water!",
				MoveOrderResult.NotOwner => "You cannot move another country's units!",
				MoveOrderResult.DestinationUnusable => "Cannot use that transport!",
				_ => ""
			});
		}
		private void MoveOtherRegimentsTo(Location<Regiment> location){
			if (!UI.IsShiftHeld){
				return;
			}
			if (Selected.Location is not TransportDeck deck){
				MoveRegimentsInLocation(Selected.Location, location);
				return;
			}
			foreach (Ship ship in deck.Transport.Location.Units){
				if (ship.Owner != Player || ship is not Transport transport){
					continue;
				}
				MoveRegimentsInLocation(transport.Deck, location);
			}
		}
		private void MoveRegimentsInLocation(Location<Regiment> currentLocation, Location<Regiment> targetLocation){
			foreach (Regiment regiment in currentLocation.Units){
				if (regiment != Selected && regiment.Owner == Player){
					Player.MoveRegimentTo(regiment, targetLocation);
				}
			}
		}
	}
}
