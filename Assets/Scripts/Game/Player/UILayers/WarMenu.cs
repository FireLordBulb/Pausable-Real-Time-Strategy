using Simulation;
using Simulation.Military;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Player {
	public class WarMenu : UILayer, IRefreshable {
		[SerializeField] private MilitaryUnitButton militaryUnitButton;
		[SerializeField] private RectTransform army;
		[SerializeField] private RectTransform navy;
		[SerializeField] private Button close;

		private bool isDone;
		private MilitaryUnitButton selectedButton;
		private RegimentType selectedRegimentType;
		private ShipType selectedShipType;
		
		private void Awake(){
			close.onClick.AddListener(() => isDone = true);
			Calendar.Instance.OnMonthTick.AddListener(Refresh);
		}
		public override void OnBegin(bool isFirstTime){
			if (!isFirstTime){
				return;
			}
			RectTransform parent = army;
			foreach (RegimentType regimentType in Player.RegimentTypes){
				parent = SetupButton(regimentType, parent);
			}
			navy.SetParent(parent, false);
			parent = navy;
			foreach (ShipType shipType in Player.ShipTypes){
				parent = SetupButton(shipType, parent);
			}
		}
		private RectTransform SetupButton<T>(UnitType<T> unitType, RectTransform parent) where T : Branch {
			MilitaryUnitButton button = Instantiate(militaryUnitButton, parent);
			button.Button.onClick.AddListener(() => SelectButton(button, unitType));
			button.gameObject.name = unitType.name;
			button.Label.text = unitType.name;
			button.Cost.text += unitType.GetCostAsString();
			return button.RectTransform;
		}
		private void SelectButton<T>(MilitaryUnitButton button, UnitType<T> unitType) where T : Branch {
			if (selectedButton == button){
				selectedButton.HideInfoBox();
				selectedButton = null;
				selectedRegimentType = null;
				selectedShipType = null;
				EventSystem.current.SetSelectedGameObject(null);
				return;
			}
			if (selectedButton != null){
				selectedButton.HideInfoBox();
			}
			selectedButton = button;
			selectedRegimentType = unitType as RegimentType;
			selectedShipType = unitType as ShipType;
			Refresh();
			selectedButton.ShowInfoBox();
			EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
		}
		
		public void Refresh(){
			if (selectedRegimentType != null){
				Refresh(selectedRegimentType);
			} else if (selectedShipType != null){
				Refresh(selectedShipType);
			}
		}
		private void Refresh<T>(UnitType<T> unitType) where T : Branch {
			selectedButton.Message.text = unitType.CanBeBuiltBy(Player) ? $"<color=green>Can be {unitType.CreatedVerb}</color>" : "<color=red>Cannot afford!</color>";
		}

		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			if (isRightClick){
				isDone = true;
			} else {
				Province clickedProvince = clickedSelectable as Province;
				clickedProvince ??= (clickedSelectable as Regiment)?.Location.Province;
				if (selectedRegimentType != null){
					Player.TryStartBuildingArmy(selectedRegimentType, clickedProvince);
					Refresh();
				} else if (selectedShipType != null){
					// TODO: TryStartBuildingNavy()
					Refresh();
				}
			}
			return UI.Selected;
		}
		public override void OnEnd(){
			Calendar.Instance.OnMonthTick.RemoveListener(Refresh);
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return isDone;
		}
	}
}
