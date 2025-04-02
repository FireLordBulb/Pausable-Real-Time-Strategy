using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Calendar = Simulation.Calendar;

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
		
		public CalendarPanel CalendarPanel => calendarPanel;
		private void Awake(){
			warButton.onClick.AddListener(() => SidePanelButtonClick(warButton, warMenu));
			economyButton.onClick.AddListener(() => SidePanelButtonClick(economyButton, economyMenu));
		}
		private void SidePanelButtonClick(Button clickedButton, UILayer menuPrefab){
			DestroyImmediate(sidePanelMenuGameObject);
			EnableButtons();
			clickedButton.enabled = false;
			UI.Push(menuPrefab);
			sidePanelMenuGameObject = UI.GetTopLayer().gameObject;
		}
		public override void OnBegin(bool isFirstTime){
			gameObject.SetActive(true);
			RefreshCountry();
			EnableButtons();
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
		private void RefreshResources(){
			gold.text = Format.FormatLargeNumber(Player.Gold, Format.FiveDigits);
			manpower.text = Format.FormatLargeNumber(Player.Manpower, Format.SevenDigits);
			sailors.text = Format.FormatLargeNumber(Player.Sailors, Format.SevenDigits);
		}
		private void EnableButtons(){
			bool mayEnable = Player != null;
			warButton.enabled = mayEnable;
			economyButton.enabled = mayEnable;
		}
		private void Start(){
			Calendar.Instance.OnPauseToggle.AddListener(pauseLabel.SetActive);
			pauseLabel.SetActive(Calendar.Instance.IsPaused);
		}
		private void Update(){
			if (sidePanelMenuGameObject == null){
				EnableButtons();
			}
			if (Player == null){
				return;
			}
			if (Player.IsDirty){
				RefreshResources();
				Player.MarkClean();
			}
		}
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			return RegularProvinceClick(clickedSelectable, isRightClick);
		}
		public override bool IsDone(){
			return false;
		}
	}
}
