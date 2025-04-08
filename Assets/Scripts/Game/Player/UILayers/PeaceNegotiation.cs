using Simulation;
using Simulation.Military;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class PeaceNegotiation : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private CountryPanel playerPanel;
		[SerializeField] private CountryPanel enemyPanel;
		[SerializeField] private Button makeDemands;
		[SerializeField] private Button whitePeace;
		[SerializeField] private Button giveConcessions;
		[SerializeField] private Button sendOffer;
		
		private PeaceTreaty treaty;
		private Country player;
		private Country enemy;
		private bool isDone;
		
		private void Awake(){
			player = Player;
			UI.Selected.OnDeselect();
			makeDemands.onClick.AddListener(() => {
				SetWinner(true);
			});
			whitePeace.onClick.AddListener(() => {
				treaty.IsWhitePeace = true;
				SetSelectedButton(whitePeace);
				foreach (Land land in treaty.AnnexedLands){
					land.Province.OnDeselect();
				}
			});
			giveConcessions.onClick.AddListener(() => {
				SetWinner(false);
			});
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
			playerPanel.SetCountry(player, Close);
			enemyPanel.SetCountry(enemy, Close);
			Refresh();
		}
		
		public void Refresh(){}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			Province clickedProvince = clickedSelectable as Province ?? (clickedSelectable as IUnit)?.Province;
			if (clickedProvince == null || clickedProvince.IsSea){
				return UI.Selected;
			}
			Land land = clickedProvince.Land;
			
			if (treaty.IsWhitePeace){
				bool isEnemyLand = land.Owner == enemy;
				if (!isEnemyLand && land.Owner != player){
					return UI.Selected;
				}
				treaty.AnnexedLands.Add(land);
				SetWinner(isEnemyLand);
				return UI.Selected;
			}
			if (land.Owner != treaty.Loser){
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
		
		private void SetWinner(bool isPlayer){
			treaty.IsWhitePeace = false;
			treaty.DidTreatyInitiatorWin = isPlayer;
			SetSelectedButton(isPlayer ? makeDemands : giveConcessions);
			foreach (Land land in treaty.AnnexedLands){
				if (land.Owner == treaty.Loser){
					land.Province.OnSelect();
				} else {
					land.Province.OnDeselect();
				}
			}
		}
		private void SetSelectedButton(Button button){
			makeDemands.interactable = true;
			whitePeace.interactable = true;
			giveConcessions.interactable = true;
			button.interactable = false;
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
