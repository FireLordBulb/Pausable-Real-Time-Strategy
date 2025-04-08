using Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class PeaceNegotiation : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private CountryPanel playerPanel;
		[SerializeField] private CountryPanel enemyPanel;
		[SerializeField] private Button sendOffer;
		
		private readonly PeaceTreaty treaty = new();
		private Country enemy;
		private bool isDone;
		
		private void Awake(){
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
			playerPanel.SetCountry(Player);
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
			return isDone || Player == null;
		}
		public void Close(){
			isDone = true;
		}
	}
}
