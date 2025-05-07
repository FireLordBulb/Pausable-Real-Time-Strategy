using System.Globalization;
using Simulation;
using Simulation.Military;
using Text;

namespace Player {
	public class TransportWindow : MilitaryUnitWindow<Transport> {
		protected override void RefreshCombatTable(){
			combatValuesTable.UpdateColumn(-1, (
				Selected.HullText),
				Selected.AttackPower.ToString("0.#", CultureInfo.InvariantCulture),
				Format.Percent(Selected.Size),
				Selected.UsedManpowerCapacityText
			);
		}
		
		protected override void OrderMove(Province province){
			ShipWindow.OrderMove(this, Selected, UI, Player, province);
		}
	}
}
