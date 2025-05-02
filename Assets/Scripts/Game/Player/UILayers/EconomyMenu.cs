using TMPro;
using UnityEngine;

namespace Player {
	public class EconomyMenu : SidePanelMenu {
		[SerializeField] private IncomeBreakdownHover incomeBreakdown;
		[SerializeField] private TextMeshProUGUI total;
		
		public override void OnBegin(bool isFirstTime){
			if (!isFirstTime){
				incomeBreakdown.RemovePanel();
			}
			incomeBreakdown.Player = Player;
			incomeBreakdown.AlwaysShow();
			total.text = incomeBreakdown.FormatAndColor(Player.GoldIncome);
		}
		public override void Refresh(){
			incomeBreakdown.Refresh();
			total.text = incomeBreakdown.FormatAndColor(Player.GoldIncome);
		}
	}
}
