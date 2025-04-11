using Simulation;
using Simulation.Military;
namespace Player {
	public class RegimentWindow : MilitaryUnitWindow<Regiment> {
		protected override void OrderMove(Province province){
			MoveOrderResult result = Player.MoveRegimentTo(Selected, province);
			SetMessage(result switch {
				MoveOrderResult.BusyRetreating => "Cannot interrupt retreat movement!",
				MoveOrderResult.NotBuilt => "Cannot move an army before it has finished recruiting!",
				MoveOrderResult.NoPath => $"Cannot move to {province} because it's separated by sea or the land of countries you're at peace with!",
				MoveOrderResult.NoAccess => "You cannot cross another country's borders when you're at peace with it!",
				MoveOrderResult.InvalidTarget => "Armies cannot walk on water!",
				MoveOrderResult.NotOwner => "You cannot move another country's units!",
				_ => ""
			});
		}
	}
}
