using System.Linq;
using System.Text;
using Simulation;
using Simulation.Military;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class PeaceNegotiation : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private CountryPanel playerPanel;
		[SerializeField] private CountryPanel enemyPanel;
		[SerializeField] private Button makeDemands;
		[SerializeField] private Button whitePeace;
		[SerializeField] private Button giveConcessions;
		[SerializeField] private TextMeshProUGUI treatyTerms;
		[SerializeField] private GameObject reparationsRow;
		[SerializeField] private TextMeshProUGUI goldTransfer;
		[SerializeField] private TextMeshProUGUI availableGold;
		[SerializeField] private Button payLess;
		[SerializeField] private Button payMore;
		[SerializeField] private Button sendOffer;
		[Header("Transfer Change Amounts")]
		[SerializeField] private float baseChange;
		[SerializeField] private float shiftMultiplier;
		[SerializeField] private float ctrlMultiplier;
		
		private PeaceTreaty treaty;
		private Country player;
		private Country enemy;
		private float otherCountryGoldTransfer;
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
				UpdateTreatyTerms();
			});
			giveConcessions.onClick.AddListener(() => {
				SetWinner(false);
			});
			
			payLess.onClick.AddListener(() => {
				treaty.GoldTransfer -= baseChange;
				Refresh();
			});
			payMore.onClick.AddListener(() => {
				treaty.GoldTransfer += baseChange;
				Refresh();
			});
			
			sendOffer.onClick.AddListener(() => {
				Player.EndWar(enemy, treaty);
				isDone = true;
				UI.Deselect();
				LayerBelow.OnEnd();
			});
		}
		
		public void Init(Country enemyCountry){
			enemy = enemyCountry;
			treaty = Player.NewPeaceTreaty(enemy);
			playerPanel.SetCountry(player, Close);
			enemyPanel.SetCountry(enemy, Close);
			Refresh();
		}

		public void Refresh(){
			UpdateGoldTransfer();
			UpdateTreatyTerms();
		}
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
			UpdateTreatyTerms();
			return UI.Selected;
		}
		public override void OnDrag(bool isRightClick){
			// Do nothing when dragging with this panel open.
		}
		
		private void SetWinner(bool isPlayer){
			treaty.IsWhitePeace = false;
			if (treaty.DidTreatyInitiatorWin != isPlayer){
				treaty.DidTreatyInitiatorWin = isPlayer;
				(treaty.GoldTransfer, otherCountryGoldTransfer) = (otherCountryGoldTransfer, treaty.GoldTransfer);
				UpdateGoldTransfer();
			}
			SetSelectedButton(isPlayer ? makeDemands : giveConcessions);
			foreach (Land land in treaty.AnnexedLands){
				if (land.Owner == treaty.Loser){
					land.Province.OnSelect();
				} else {
					land.Province.OnDeselect();
				}
			}
			reparationsRow.SetActive(true);
			UpdateTreatyTerms();
		}
		private void UpdateGoldTransfer(){
			availableGold.text = $"/{Format.FormatLargeNumber(treaty.Loser.Gold, Format.FiveDigits)}<color=yellow>G</color>";
			if (treaty.GoldTransfer <= 0){
				treaty.GoldTransfer = 0;
				payLess.interactable = false;
			} else {
				payLess.interactable = true;
			}
			if (treaty.GoldTransfer >= treaty.Loser.Gold){
				treaty.GoldTransfer = treaty.Loser.Gold;
				payMore.interactable = false;
			} else {
				payMore.interactable = true;
			}
			goldTransfer.text = $"{Format.FormatLargeNumber(treaty.GoldTransfer, Format.FiveDigits)}<color=yellow>G</color>";
		}
		private void SetSelectedButton(Button button){
			makeDemands.interactable = true;
			whitePeace.interactable = true;
			giveConcessions.interactable = true;
			button.interactable = false;
		}
		private void UpdateTreatyTerms(){
			if (treaty.IsWhitePeace){
				treatyTerms.text = "- White Peace";
				reparationsRow.SetActive(false);
				return;
			}
			StringBuilder builder = new($"- {treaty.Loser} concedes defeat");
			int annexedProvinceCount = treaty.AnnexedLands.Count(land => land.Owner == treaty.Loser);
			if (0 < annexedProvinceCount){
				builder.Append($"\n- {treaty.Winner} annexes {annexedProvinceCount} province{(annexedProvinceCount == 1 ? "" : 's')} from {treaty.Loser}");
			}
			if (0 < treaty.GoldTransfer){
				builder.Append($"\n- {treaty.Loser} pays {Format.FormatLargeNumber(treaty.GoldTransfer, Format.FiveDigits)} gold in reparations to {treaty.Winner}");
			}
			treatyTerms.text = builder.ToString();
			reparationsRow.SetActive(true);
		}
		
		public override void OnEnd(){
			foreach (Land land in treaty.AnnexedLands){
				land.Province.OnDeselect();
			}
			UI.Selected?.OnSelect();
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
