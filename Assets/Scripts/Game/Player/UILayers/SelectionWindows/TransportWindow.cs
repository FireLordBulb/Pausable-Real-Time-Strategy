using System.Globalization;
using Simulation;
using Simulation.Military;
using Text;
using UnityEngine;

namespace Player {
	public class TransportWindow : MilitaryUnitWindow<Transport> {
		[Header("TransportDeck")]
		[SerializeField] private UnitListScrollView unitListScrollView;
		protected override void RefreshCombatTable(){
			combatValuesTable.UpdateColumn(-1, (
				Selected.HullText),
				Selected.AttackPower.ToString("0.#", CultureInfo.InvariantCulture),
				Format.Percent(Selected.Size),
				Selected.UsedManpowerCapacityText
			);
			unitListScrollView.Refresh(Selected.Deck.Units, UI);
		}
		
		protected override void OrderMove(Province province){
			ShipWindow.OrderMove(this, Selected, UI, Player, province);
		}
	}
}
