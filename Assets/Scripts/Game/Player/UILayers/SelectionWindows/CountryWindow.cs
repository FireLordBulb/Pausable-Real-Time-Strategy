using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountryWindow : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private Image flag;
		[SerializeField] private ValueTable valueTable;
		[SerializeField] private string[] valueNames;
		[SerializeField] private GameObject diplomacy;
		[SerializeField] private TextMeshProUGUI statusLabel;
		[SerializeField] private TextMeshProUGUI statusDescription;
		[SerializeField] private Button declareWar;
		[SerializeField] private Button select;
		
		private Country country;
		private DiplomaticStatus diplomaticStatus;
		
		private void Awake(){
			country = UI.SelectedCountry;
			title.text = $"{country.Name}";
			flag.material = new Material(flag.material){
				color = country.MapColor
			};
			
			valueTable.Generate(-1, valueNames);
			SetupDiplomacy();
			SetupSelectButton();
			Refresh();
			Calendar.Instance.OnMonthTick.AddListener(Refresh);
			select.onClick.AddListener(() => {
				UI.PlayAs(country);
				UI.Deselect();
				UI.ClearSelectHistory();
			});
			
			country.OnSelect();
		}

		public void Refresh(){
			if (UI.HasPlayerCountryChanged){
				SetupDiplomacy();
				SetupSelectButton();
			}
			valueTable.UpdateColumn(0, (
				Format.FormatLargeNumber(country.ProvinceCount, Format.SevenDigits)),	
				Format.FormatLargeNumber(country.RegimentCount, Format.FiveDigits),	
				Format.FormatLargeNumber(country.ShipCount, Format.FiveDigits),	
				Format.FormatLargeNumber(country.Gold, Format.FiveDigits),	
				Format.FormatLargeNumber(country.Manpower, Format.SevenDigits),	
				Format.FormatLargeNumber(country.Sailors, Format.SevenDigits)
			);
			RefreshDiplomacy();
		}

		private void SetupDiplomacy(){
			if (Player != null && Player != country){
				diplomaticStatus = Player.GetDiplomaticStatus(country);
				declareWar.onClick.AddListener(() => {
					diplomaticStatus.DeclareWar();
					RefreshDiplomacy();
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
			if (Player == country){
				statusLabel.text = "This is your country";
				statusDescription.text = "";
				declareWar.gameObject.SetActive(false);
				SetDailyDiplomacyRefresh(false);
			} else {
				statusLabel.text = $"Diplomatic status:";
				if (diplomaticStatus.IsAtWar){
					statusDescription.text = "At WAR";
					SetDailyDiplomacyRefresh(false);
				} else {
					if (diplomaticStatus.TruceDaysLeft == 0){
						statusDescription.text = "At Peace (no truce)";
						SetDailyDiplomacyRefresh(false);
					} else {
						statusDescription.text = $"At Peace, truce for {diplomaticStatus.TruceDaysLeft} more days";
						SetDailyDiplomacyRefresh(true);
					}
				}
				declareWar.gameObject.SetActive(true);
				declareWar.interactable = diplomaticStatus.CanDeclareWar();
			}
		}
		private bool isRefreshingDiplomacyDaily;
		private void SetDailyDiplomacyRefresh(bool doRefresh){
			if (isRefreshingDiplomacyDaily == doRefresh){
				return;
			}
			isRefreshingDiplomacyDaily = doRefresh;
			if (isRefreshingDiplomacyDaily){
				Calendar.Instance.OnDayTick.AddListener(RefreshDiplomacy);
			} else {
				Calendar.Instance.OnDayTick.RemoveListener(RefreshDiplomacy);
			}
		}
		
		public override void OnEnd(){
			Calendar.Instance.OnMonthTick.RemoveListener(Refresh);
			Calendar.Instance.OnDayTick.RemoveListener(RefreshDiplomacy);
			country.OnDeselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.SelectedCountry != country;
		}
		public void Close(){
			UI.Deselect(country);
		}
	}
}
