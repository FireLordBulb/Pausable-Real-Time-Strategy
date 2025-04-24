using Simulation;
using Simulation.Military;
namespace Player {
	public class RegimentWindow : MilitaryUnitWindow<Regiment> {
		protected override bool DoBypassDefaultBehaviour(ISelectable clickedSelectable){
			if (clickedSelectable is not Transport transport){
				return false;
			}
			TransportDeck deck = transport.Deck;
			MoveOrderResult result = Player.MoveRegimentTo(Selected, deck);
			SetMessageFromResult(result, deck.Name);
			if (!UI.IsShiftHeld){
				return true;
			}
			foreach (Regiment regiment in Selected.Location.Units){
				if (regiment != Selected && regiment.Owner == Player){
					Player.MoveRegimentTo(regiment, deck);
				}
			}
			return true;
		}
		protected override void OrderMove(Province province){
			Location<Regiment> location = province.IsLand ? province.Land.ArmyLocation : null;
			MoveOrderResult result = Player.MoveRegimentTo(Selected, location);
			SetMessageFromResult(result, province.Name);
			if (!UI.IsShiftHeld){
				return;
			}
			foreach (Regiment regiment in Selected.Location.Units){
				if (regiment != Selected && regiment.Owner == Player){
					Player.MoveRegimentTo(regiment, location);
				}
			}
		}
		private void SetMessageFromResult(MoveOrderResult result, string targetName){
			SetMessage(result switch {
				MoveOrderResult.BusyRetreating => "Cannot interrupt retreat movement!",
				MoveOrderResult.NotBuilt => "Cannot move an army before it has finished recruiting!",
				MoveOrderResult.NoPath => $"Cannot move to {targetName} because it's separated by sea or the land of countries you're at peace with!",
				MoveOrderResult.NoAccess => "You cannot cross another country's borders when you're at peace with it!",
				MoveOrderResult.InvalidTarget => "Armies cannot walk on water!",
				MoveOrderResult.NotOwner => "You cannot move another country's units!",
				_ => ""
			});
		}
	}
}
