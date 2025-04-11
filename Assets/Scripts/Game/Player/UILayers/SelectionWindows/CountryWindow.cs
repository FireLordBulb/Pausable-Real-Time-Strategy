using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountryWindow : UILayer, IRefreshable, IClosableWindow {
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
		
		private Country country;
		private DiplomaticStatus diplomaticStatus;
		private PeaceNegotiation peaceNegociation;
		
		public override void OnBegin(bool isFirstTime){
			if (!isFirstTime){
				return;
			}
			country = UI.SelectedCountry;
			title.text = $"{country.Name}";
			flag.material = new Material(flag.material){
				color = country.MapColor
			};
			
			valueTable.Generate(-1, valueNames);
			SetupDiplomacy();
			SetupSelectButton();
			Refresh();
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
			if (peaceNegociation != null){
				peaceNegociation.Refresh();
			}
		}

		private void SetupDiplomacy(){
			if (Player != null && Player != country){
				diplomaticStatus = Player.GetDiplomaticStatus(country);
				declareWar.onClick.AddListener(() => {
					diplomaticStatus.DeclareWar();
					RefreshDiplomacy();
				});
				makePeace.onClick.AddListener(() => {
					if (peaceNegociation == null){
						UI.Push(peaceNegociationPrefab);
						peaceNegociation = (PeaceNegotiation)UI.GetTopLayer();
						peaceNegociation.Init(country);
					} else {
						peaceNegociation.Close();
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
			if (Player == country){
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
				Calendar.OnDayTick.AddListener(RefreshDiplomacy);
			} else {
				Calendar.OnDayTick.RemoveListener(RefreshDiplomacy);
			}
		}
		
		public override void OnEnd(){
			if (peaceNegociation != null){
				peaceNegociation.OnEnd();
			}
			country.OnDeselect();
			Calendar.OnDayTick.RemoveListener(RefreshDiplomacy);
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
