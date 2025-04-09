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
		[Header("Reparation UI Elements")]
		[SerializeField] private GameObject reparationsRow;
		[SerializeField] private TextMeshProUGUI goldTransfer;
		[SerializeField] private TextMeshProUGUI availableGold;
		[SerializeField] private Button payLess;
		[SerializeField] private Button payMore;
		[Header("Offer Peace UI Elements")]
		[SerializeField] private Button sendOffer;
		[SerializeField] private TextMeshProUGUI sendButtonText;
		[SerializeField] private TextMeshProUGUI acceptDescription;
		[SerializeField] private TextMeshProUGUI acceptValue;
		[Space]
		[Header("Transfer Change Amounts")]
		[SerializeField] private float baseChange;
		[SerializeField] private float shiftMultiplier;
		[SerializeField] private float ctrlMultiplier;
		[Header("Offer Peace Times")]
		[SerializeField] private int responseDays;
		[SerializeField] private int rejectedSendBlockDays;
		
		private PeaceTreaty treaty;
		private Country player;
		private Country enemy;
		private float otherCountryGoldTransfer;
		private int daysUntilResponse;
		private bool isDone;
		
		private float ChangeAmount => baseChange*(UI.IsShiftHeld ? shiftMultiplier : 1)*(UI.IsControlHeld ? ctrlMultiplier : 1);
		
		private void Awake(){
			makeDemands.onClick.AddListener(() => {
				SetWinner(true);
			});
			whitePeace.onClick.AddListener(() => {
				treaty.IsWhitePeace = true;
				SetSelectedButton(whitePeace);
				foreach (Land land in treaty.AnnexedLands){
					land.Province.OnDeselect();
				}
				RefreshTreatyTerms();
			});
			giveConcessions.onClick.AddListener(() => {
				SetWinner(false);
			});
			
			payLess.onClick.AddListener(() => {
				treaty.GoldTransfer -= ChangeAmount;
				Refresh();
			});
			payMore.onClick.AddListener(() => {
				treaty.GoldTransfer += ChangeAmount;
				Refresh();
			});
			
			sendOffer.onClick.AddListener(() => {
				sendButtonText.text = "Awaiting Answer...";
				daysUntilResponse = responseDays;
				Calendar.Instance.OnDayTick.AddListener(AwaitAnswer);
			});
		}
		private void AwaitAnswer(){
			daysUntilResponse--;
			if (daysUntilResponse > 0){
				return;
			}
			Calendar.Instance.OnDayTick.RemoveListener(AwaitAnswer);
			// Random value as placeholder. TODO: Ask AI class to evaluate.
			int value = Random.Range(-200, +200);
			if (value < 0){
				sendButtonText.text = "Offer Peace";
				RefreshAcceptance(value);
				return;
			}
			Player.EndWar(enemy, treaty);
			isDone = true;
			UI.Deselect();
			LayerBelow.OnEnd();
		}
		
		public void Init(Country enemyCountry){
			player = Player;
			enemy = enemyCountry;
			treaty = Player.NewPeaceTreaty(enemy);
			playerPanel.SetCountry(player, Close);
			enemyPanel.SetCountry(enemy, Close);
			Refresh();
			UI.Selected.OnDeselect();
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
			RefreshTreatyTerms();
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
				RefreshGoldTransfer();
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
			RefreshTreatyTerms();
		}
		
		public void Refresh(){
			RefreshGoldTransfer();
			RefreshTreatyTerms();
		}
		private void RefreshGoldTransfer(){
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
		private void RefreshTreatyTerms(){
			// Random value as placeholder. TODO: Ask AI class to evaluate.
			int value = Random.Range(-200, +200);
			RefreshAcceptance(value);
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
		private void RefreshAcceptance(int value){
			acceptValue.text = Format.Signed(value);
			if (value < 0){
				acceptDescription.text = "Would Reject Treaty";
				acceptDescription.color = acceptValue.color = Color.red;
			} else {
				acceptDescription.text = "Would Accept Treaty";
				acceptDescription.color = acceptValue.color = Color.green;
			}
		}
		
		public override void OnEnd(){
			Calendar.Instance.OnDayTick.RemoveListener(AwaitAnswer);
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
