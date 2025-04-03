using Simulation;
using Simulation.Military;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Player {
	public class WarMenu : UILayer {
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
			selectedButton.Message.text = unitType.CanBeBuiltBy(Player) ? $"<color=green>Can be {unitType.CreatedVerb}</color>" : "<color=red>Cannot afford!</color>";
			selectedButton.ShowInfoBox();
			EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
		}
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			if (isRightClick){
				isDone = true;
			} else {
				Province clickedProvince = clickedSelectable as Province;
				clickedProvince ??= (clickedSelectable as Regiment)?.Location.Province;
				if (selectedRegimentType != null){
					Player.TryStartBuildingArmy(selectedRegimentType, clickedProvince);
				} else if (selectedShipType != null){
					// TODO: TryStartBuildingNavy()
				}
			}
			return UI.Selected;
		}	
		public override bool IsDone(){
			base.IsDone();
			return isDone;
		}
	}
}
