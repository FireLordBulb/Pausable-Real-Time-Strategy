using AI;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountryWindow : SelectionWindow<Country> {
		[SerializeField] private PeaceNegotiation peaceNegociationPrefab;
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image flag;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private GameObject diplomacy;
		[SerializeField] private TextMeshProUGUI statusLabel;
		[SerializeField] private TextMeshProUGUI statusDescription;
		[SerializeField] private Button declareWar;
		[SerializeField] private Button makePeace;
		[SerializeField] private Button select;
		
		private DiplomaticStatus diplomaticStatus;
		private PeaceNegotiation peaceNegotiation;
		
		private void Awake(){
			select.onClick.AddListener(() => {
				UI.PlayAs(Selected);
				UI.Deselect(Selected);
			});
		}
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			title.text = $"{Selected.Name}";
			flag.material = new Material(flag.material){
				color = Selected.MapColor
			};
			
			valueTable.Generate(-1, valueNames);
			SetupDiplomacy();
			SetupSelectButton();
			Refresh();
		}
		
		public override void Refresh(){
			if (UI.HasPlayerCountryChanged){
				SetupDiplomacy();
				SetupSelectButton();
			}
			valueTable.UpdateColumn(0, (
				Format.FormatLargeNumber(Selected.ProvinceCount, Format.SevenDigits)),	
				Format.FormatLargeNumber(Selected.Regiments.Count, Format.FiveDigits),	
				Format.FormatLargeNumber(Selected.Ships.Count, Format.FiveDigits),	
				Format.FormatLargeNumber(Selected.Gold, Format.FiveDigits),	
				Format.FormatLargeNumber(Selected.Manpower, Format.SevenDigits),	
				Format.FormatLargeNumber(Selected.Sailors, Format.SevenDigits)
			);
			RefreshDiplomacy();
			if (peaceNegotiation != null){
				peaceNegotiation.Refresh();
			}
		}

		private void SetupDiplomacy(){
			if (Player != null && Player != Selected){
				diplomaticStatus = Player.GetDiplomaticStatus(Selected);
				declareWar.onClick.AddListener(() => {
					Player.DeclareWar(Selected);
					AIController.OnWarStart(UI.GetAI(Player), UI.GetAI(Selected));
					RefreshDiplomacy();
				});
				makePeace.onClick.AddListener(() => {
					if (peaceNegotiation == null){
						peaceNegotiation = UI.Push(peaceNegociationPrefab);
					} else {
						peaceNegotiation.Close();
					}
				});
			} else {
				diplomaticStatus = null;
				declareWar.onClick.RemoveAllListeners();
			}
		}
		private void SetupSelectButton(){
			select.gameObject.SetActive(Player == null || UI.CanSwitchCountry);
		}
		
		private void RefreshDiplomacy(){
			if (Player == null){
				diplomacy.SetActive(false);
				SetDailyDiplomacyRefresh(false);
				return;
			}
			diplomacy.SetActive(true);
			if (Player == Selected){
				statusLabel.text = "This is your country";
				statusDescription.text = "";
				declareWar.gameObject.SetActive(false);
				makePeace.gameObject.SetActive(false);
				SetDailyDiplomacyRefresh(false);
				return;
			}
			statusLabel.text = $"Diplomatic status:";
			if (diplomaticStatus.IsAtWar){
				statusDescription.text = "At WAR";
				SetDailyDiplomacyRefresh(false);
				declareWar.gameObject.SetActive(false);
				makePeace.gameObject.SetActive(true);
			} else {
				if (diplomaticStatus.TruceDaysLeft == 0){
					statusDescription.text = "At Peace (no truce)";
					SetDailyDiplomacyRefresh(false);
				} else {
					statusDescription.text = $"At Peace, truce for {diplomaticStatus.TruceDaysLeft} more days";
					SetDailyDiplomacyRefresh(true);
				}
				declareWar.gameObject.SetActive(true);
				makePeace.gameObject.SetActive(false);
				declareWar.interactable = diplomaticStatus.CanDeclareWar(Selected);
			}
		}
		private bool isRefreshingDiplomacyDaily;
		private void SetDailyDiplomacyRefresh(bool doRefresh){
			if (isRefreshingDiplomacyDaily == doRefresh){
				return;
			}
			isRefreshingDiplomacyDaily = doRefresh;
			if (isRefreshingDiplomacyDaily){
				Calendar.OnDayTick.AddListener(RefreshDiplomacy);
			} else {
				Calendar.OnDayTick.RemoveListener(RefreshDiplomacy);
			}
		}
		
		public override void OnEnd(){
			if (peaceNegotiation != null){
				peaceNegotiation.OnEnd();
			}
			Calendar.OnDayTick.RemoveListener(RefreshDiplomacy);
			base.OnEnd();
		}
	}
}
