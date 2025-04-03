using Simulation;
using Simulation.Military;
namespace Player {
	public class ShipWindow : MilitaryUnitWindow<Ship, Navy> {
		protected override void OrderMove(Province province){
			MoveOrderResult result;
			if (province.IsSea){
				result = Player.MoveFleetTo(Unit, province.Sea.NavyLocation);
			} else if (province.IsCoast){
				result = Player.MoveFleetTo(Unit, GetHarbor(province));
			} else {
				result = MoveOrderResult.InvalidTarget;
			}
			SetMessage(result switch {
				MoveOrderResult.NotBuilt => "Cannot move a navy before it has finished constructing!",
				MoveOrderResult.NoPath => $"Cannot move to {province} because the path is blocked by landmass!",
				MoveOrderResult.NoAccess => "You cannot enter another country's harbors when unless at war with the country!",
				MoveOrderResult.InvalidTarget => "Navies cannot sail on land!",
				MoveOrderResult.NotOwner => "You cannot move another country's units!",
				_ => ""
			});
		}
	}
}
