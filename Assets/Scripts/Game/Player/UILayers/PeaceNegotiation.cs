using Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class PeaceNegotiation : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private CountryPanel playerPanel;
		[SerializeField] private CountryPanel enemyPanel;
		[SerializeField] private Button sendOffer;
		
		private PeaceTreaty treaty;
		private Country player;
		private Country enemy;
		private bool isDone;
		
		private void Awake(){
			player = Player;
			sendOffer.onClick.AddListener(() => {
				Player.EndWar(enemy, treaty);
				isDone = true;
				UI.Deselect();
				LayerBelow.OnEnd();
			});
			Calendar.Instance.OnMonthTick.AddListener(Refresh);
		}
		public void Init(Country enemyCountry){
			enemy = enemyCountry;
			treaty = Player.NewPeaceTreaty(enemy);
			playerPanel.SetCountry(player);
			enemyPanel.SetCountry(enemy);
			Refresh();
		}
		
		public void Refresh(){}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			return UI.Selected;
		}
		public override void OnEnd(){
			Calendar.Instance.OnMonthTick.RemoveListener(Refresh);
			base.OnEnd();
		}
		public override bool IsDone(){
			return isDone || Player != player || UI.SelectedCountry != enemy;
		}
		public void Close(){
			isDone = true;
		}
	}
}
