using System;
using Simulation;
using Text;
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
		[SerializeField] private IncomeBreakdownHover[] breakdowns;
		[Header("SidePanel")]
		[SerializeField] private SidePanelButton[] sidePanelButtons;
		[Header("OtherPanels")]
		[SerializeField] private CalendarPanel calendarPanel;
		[SerializeField] private MapModeSelect mapModeSelect;
		[Space]
		[SerializeField] private int resourceMaxCharacters;
		
		private SidePanelMenu sidePanelMenu;
		
		public CalendarPanel CalendarPanel => calendarPanel;
		public MapModeSelect MapModeSelect => mapModeSelect;
		
		private void Awake(){
			foreach (SidePanelButton sidePanelButton in sidePanelButtons){
				sidePanelButton.button.onClick.AddListener(() => SidePanelButtonClick(sidePanelButton));
			}
		}
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			mapModeSelect.CameraInput = UI.CameraInput;
			RefreshCountry();
			SetButtonsInteractable();
		}
		public override void OnBegin(bool isFirstTime){
			gameObject.SetActive(true);
		}
		private void SidePanelButtonClick(SidePanelButton sidePanelButton){
			if (sidePanelMenu != null){
				sidePanelMenu.Close();
			}
			SetButtonsInteractable();
			sidePanelButton.button.interactable = false;
			sidePanelMenu = UI.Push(sidePanelButton.menu);
		}

		public void OpenSidePanel(int index){
			if (index < 0 || sidePanelButtons.Length <= index){
				return;
			}
			if (sidePanelButtons[index].button.interactable){
				SidePanelButtonClick(sidePanelButtons[index]);
			} else if (sidePanelMenu != null){
				sidePanelMenu.Close();
			}
		}
		
		public void RefreshCountry(){
			foreach (IncomeBreakdownHover breakdown in breakdowns){
				breakdown.Player = Player;
			}
			if (Player == null){
				countryFlag.material = new Material(countryFlag.material){
					color = Color.magenta
				};
				UI.Links.Remove(countryFlag);
				countryName.text = "<i>Observing</i>";
				sailors.text = manpower.text = gold.text = "N/A";
				return;
			}
			countryFlag.material = new Material(countryFlag.material){
				color = Player.MapColor
			};
			UI.Links.Add(countryFlag, Player, true);
			countryName.text = Player.gameObject.name;
			RefreshResources();
		}
		public void RefreshResources(){
			if (Player == null){
				return;
			}
			gold.text = Format.FormatLargeNumber(Player.Gold, resourceMaxCharacters);
			manpower.text = Format.FormatLargeNumber(Player.Manpower, resourceMaxCharacters);
			sailors.text = Format.FormatLargeNumber(Player.Sailors, resourceMaxCharacters);
			foreach (IncomeBreakdownHover breakdown in breakdowns){
				breakdown.Refresh();
			}
			if (sidePanelMenu != null){
				sidePanelMenu.Refresh();
			}
		}
		private void SetButtonsInteractable(){
			bool mayInteract = Player != null;
			foreach (SidePanelButton sidePanelButton in sidePanelButtons){
				sidePanelButton.button.interactable = mayInteract;
			}
		}
		private void Start(){
			Calendar.OnPauseToggle.AddListener(pauseLabel.SetActive);
			pauseLabel.SetActive(Calendar.IsPaused);
		}
		private void Update(){
			if (sidePanelMenu == null){
				SetButtonsInteractable();
			}
		}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			if (clickedSelectable is not Province clickedProvince){
				return clickedSelectable == UI.Selected ? null : clickedSelectable;
			}
			if (isRightClick){
				return clickedProvince.IsSea ? null : ReferenceEquals(clickedProvince.Land.Owner, UI.Selected) ? null : clickedProvince.Land.Owner;
			}
			return ReferenceEquals(clickedProvince, UI.Selected) ? null : clickedProvince;
		}
		public override bool IsDone(){
			return false;
		}

		[Serializable]
		private struct SidePanelButton {
			public Button button;
			public SidePanelMenu menu;
		}
	}
}
