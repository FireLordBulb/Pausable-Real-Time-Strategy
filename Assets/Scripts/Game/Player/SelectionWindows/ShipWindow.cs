using System.Globalization;
using Simulation;
using Simulation.Military;
using Text;

namespace Player {
	public class ShipWindow : MilitaryUnitWindow<Ship> {
		protected override void RefreshCombatTable(){
			combatValuesTable.UpdateColumn(-1, (
				Selected.HullText),
				Selected.AttackPower.ToString("0.#", CultureInfo.InvariantCulture),
				Format.Percent(Selected.Size)
			);
		}
		
		protected override void OrderMove(Province province){
			OrderMove(this, Selected, UI, Player, province);
		}
		internal static void OrderMove<TShip>(MilitaryUnitWindow<TShip> window, TShip selectedShip, UIStack ui, Country player, Province province) where TShip : Ship {
			Location<Ship> location;
			if (province.IsSea){
				location = province.Sea.NavyLocation;
			} else if (province.IsCoast){
				location = ui.GetHarbor(province);
			} else {
				location = null;
			}
			MoveOrderResult result = player.MoveFleetTo(selectedShip, location);
			window.SetMessage(result switch {
				MoveOrderResult.BusyRetreating => "Cannot interrupt retreat movement!",
				MoveOrderResult.NotBuilt => "Cannot move a navy before it has finished constructing!",
				MoveOrderResult.NoPath => $"Cannot move to {province} because the path is blocked by landmass!",
				MoveOrderResult.NoAccess => "You cannot enter another country's harbors when unless at war with the country!",
				MoveOrderResult.InvalidTarget => "Navies cannot sail on land!",
				MoveOrderResult.NotOwner => "You cannot move another country's units!",
				_ => ""
			});
			if (!ui.IsShiftHeld){
				return;
			}
			foreach (Ship ship in selectedShip.Location.Units){
				if (ship != selectedShip && ship.Owner == player){
					player.MoveFleetTo(ship, location);
				}
			}
		}
	}
}
