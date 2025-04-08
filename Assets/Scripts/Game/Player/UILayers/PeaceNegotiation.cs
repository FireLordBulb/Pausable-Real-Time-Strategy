using Simulation;
using Simulation.Military;
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
			UI.Selected.OnDeselect();
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
			// Default to treaties where the player won, since hopefully that happens more often than losing.
			treaty.DidTreatyInitiatorWin = true;
			playerPanel.SetCountry(player);
			enemyPanel.SetCountry(enemy);
			Refresh();
		}
		
		public void Refresh(){}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			Province clickedProvince = clickedSelectable as Province ?? (clickedSelectable as IUnit)?.Province;
			if (clickedProvince == null || clickedProvince.IsSea){
				return UI.Selected;
			}
			Land land = clickedProvince.Land;
			if (land.Owner != player && land.Owner != enemy){
				return UI.Selected;
			}
			if (treaty.AnnexedLands.Contains(land)){
				treaty.AnnexedLands.Remove(land);
				land.Province.OnDeselect();
			} else {
				treaty.AnnexedLands.Add(land);
				land.Province.OnSelect();
			}
			return UI.Selected;
		}
		public override void OnEnd(){
			foreach (Land land in treaty.AnnexedLands){
				land.Province.OnDeselect();
			}
			UI.Selected?.OnSelect();
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
