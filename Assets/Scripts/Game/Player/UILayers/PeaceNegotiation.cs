using System.Linq;
using System.Text;
using AI;
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
		[SerializeField] private TextMeshProUGUI sendingBlocked;
		[Space]
		[Header("Transfer Change Amounts")]
		[SerializeField] private float baseChange;
		[SerializeField] private float shiftMultiplier;
		[SerializeField] private float ctrlMultiplier;
		[Header("Offer Peace Times")]
		[SerializeField] private int responseDays;
		[SerializeField] private int rejectedSendBlockDays;
		
		private PeaceTreaty treaty;
		private PeaceTreaty pendingTreaty;
		private Country player;
		private Country enemy;
		private AIController enemyAI;
		private float otherCountryGoldTransfer;
		private int daysUntilResponse;
		private int daysUntilSendingUnblocked;
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
				sendOffer.interactable = false;
				sendButtonText.text = "Awaiting Answer...";
				daysUntilResponse = responseDays;
				pendingTreaty = treaty.Copy();
				Calendar.OnDayTick.AddListener(AwaitAnswer);
			});
		}
		private void AwaitAnswer(){
			daysUntilResponse--;
			if (daysUntilResponse > 0){
				return;
			}
			Calendar.OnDayTick.RemoveListener(AwaitAnswer);
			int responseValue = enemyAI.EvaluatePeaceOffer(pendingTreaty); 
			if (responseValue < 0){
				sendButtonText.text = "Offer Peace";
				acceptValue.text = Format.Signed(responseValue);
				acceptDescription.text = "Peace Offer Rejected";
				acceptDescription.color = acceptValue.color = Color.red;
				daysUntilSendingUnblocked = rejectedSendBlockDays;
				RefreshTreatyTerms();
				RefreshSendingBlockedText();
				Calendar.OnDayTick.AddListener(BlockedCountdown);
				return;
			}
			Player.EndWar(enemy, pendingTreaty);
			AIController.OnWarEnd(UI.GetAI(Player), enemyAI);
			isDone = true;
			UI.Deselect(enemy);
			LayerBelow.OnEnd();
		}
		private void BlockedCountdown(){
			daysUntilSendingUnblocked--;
			if (daysUntilSendingUnblocked > 0){
				RefreshSendingBlockedText();
				return;
			}
			Calendar.OnDayTick.RemoveListener(BlockedCountdown);
			sendOffer.interactable = true;
			sendingBlocked.text = "";
			RefreshTreatyTerms();
		}
		private void RefreshSendingBlockedText(){
			sendingBlocked.text = $"May send another peace offer in {daysUntilSendingUnblocked} days";
		}
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			player = Player;
			enemy = (Country)UI.Selected;
			enemyAI = UI.GetAI(enemy);
			treaty = Player.NewPeaceTreaty(enemy);
			playerPanel.SetCountry(player, UI, Close);
			enemyPanel.SetCountry(enemy, UI, Close);
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
			RefreshAcceptance();
			if (treaty.IsWhitePeace){
				SetTreatyTermsText("- White Peace");
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
			SetTreatyTermsText(builder.ToString());
			reparationsRow.SetActive(true);
		}
		private void SetTreatyTermsText(string text){
			// Don't alter the text of the treaty if a response is pending.
			if (daysUntilResponse > 0){
				return;
			}
			treatyTerms.text = text;
		}
		private void RefreshAcceptance(){
			// Don't updated the acceptance value when sending is blocked, keep the value that resulted in rejection visible until sending is available again.
			if (daysUntilSendingUnblocked > 0 || daysUntilResponse > 0){
				return;
			}
			int acceptanceValue = enemyAI.EvaluatePeaceOffer(treaty);
			acceptValue.text = Format.Signed(acceptanceValue);
			if (acceptanceValue < 0){
				acceptDescription.text = "Would Reject Treaty";
				acceptDescription.color = acceptValue.color = Color.red;
			} else {
				acceptDescription.text = "Would Accept Treaty";
				acceptDescription.color = acceptValue.color = Color.green;
			}
		}
		
		public override void OnEnd(){
			Calendar.OnDayTick.RemoveListener(AwaitAnswer);
			Calendar.OnDayTick.RemoveListener(BlockedCountdown);
			foreach (Land land in treaty.AnnexedLands){
				land.Province.OnDeselect();
			}
			UI.Selected?.OnSelect();
			base.OnEnd();
		}
		public override bool IsDone(){
			return isDone || Player != player || !ReferenceEquals(UI.Selected, enemy);
		}
		public void Close(){
			isDone = true;
		}
	}
}
