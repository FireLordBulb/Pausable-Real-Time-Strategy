using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class HUD : UILayer {
		[SerializeField] private GameObject pauseLabel;
		[Header("TopPanel")]
		[SerializeField] private Image countryFlag;
		[SerializeField] private TextMeshProUGUI countryName;
		[SerializeField] private TextMeshProUGUI gold;
		[SerializeField] private TextMeshProUGUI manpower;
		[SerializeField] private TextMeshProUGUI sailors;
		[Header("SidePanel")]
		[SerializeField] private UILayer warMenu;
		[SerializeField] private UILayer economyMenu;
		[SerializeField] private Button warButton;
		[SerializeField] private Button economyButton;
		[Header("CalendarPanel")]
		[SerializeField] private CalendarPanel calendarPanel;
		
		private GameObject sidePanelMenuGameObject;
		private IRefreshable sidePanelMenuRefreshable;
		
		public CalendarPanel CalendarPanel => calendarPanel;
		private void Awake(){
			warButton.onClick.AddListener(() => SidePanelButtonClick(warButton, warMenu));
			economyButton.onClick.AddListener(() => SidePanelButtonClick(economyButton, economyMenu));
		}
		private void SidePanelButtonClick(Button clickedButton, UILayer menuPrefab){
			DestroyImmediate(sidePanelMenuGameObject);
			SetButtonsInteractable();
			clickedButton.interactable = false;
			UI.Push(menuPrefab);
			sidePanelMenuGameObject = UI.GetTopLayer().gameObject;
			sidePanelMenuRefreshable = sidePanelMenuGameObject.GetComponent<IRefreshable>();
		}
		// ReSharper disable Unity.PerformanceAnalysis // OnBegin isn't called every frame.
		public override void OnBegin(bool isFirstTime){
			gameObject.SetActive(true);
			RefreshCountry();
			SetButtonsInteractable();
		}
		public void RefreshCountry(){
			if (Player == null){
				countryFlag.material = new Material(countryFlag.material){
					color = Color.magenta
				};
				countryName.text = "<i>Observing</i>";
				sailors.text = manpower.text = gold.text = "N/A";
				return;
			}
			countryFlag.material = new Material(countryFlag.material){
				color = Player.MapColor
			};
			countryName.text = Player.gameObject.name;
			RefreshResources();
		}
		public void RefreshResources(){
			if (Player == null){
				return;
			}
			gold.text = Format.FormatLargeNumber(Player.Gold, Format.FiveDigits);
			manpower.text = Format.FormatLargeNumber(Player.Manpower, Format.SevenDigits);
			sailors.text = Format.FormatLargeNumber(Player.Sailors, Format.SevenDigits);
			sidePanelMenuRefreshable?.Refresh();
		}
		private void SetButtonsInteractable(){
			bool mayInteract = Player != null;
			warButton.interactable = mayInteract;
			economyButton.interactable = mayInteract;
		}
		private void Start(){
			Calendar.OnPauseToggle.AddListener(pauseLabel.SetActive);
			pauseLabel.SetActive(Calendar.IsPaused);
		}
		private void Update(){
			if (sidePanelMenuGameObject == null){
				SetButtonsInteractable();
			}
		}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			if (clickedSelectable is not Province clickedProvince){
				return clickedSelectable == UI.Selected ? null : clickedSelectable;
			}
			if (isRightClick){
				return clickedProvince.IsSea ? null : clickedProvince.Land.Owner == UI.SelectedCountry ? null : clickedProvince.Land.Owner;
			}
			return clickedProvince == UI.SelectedProvince ? null : clickedProvince;
		}
		public override bool IsDone(){
			return false;
		}
	}
}
