using Simulation;
using Simulation.Military;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class WarMenu : UILayer {
		[SerializeField] private MilitaryUnitButton militaryUnitButton;
		[SerializeField] private Transform rows;
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
			Vector2 positionOffset = new(0, -army.rect.height);
			Vector2 rowPosition = positionOffset;
			foreach (RegimentType regimentType in Player.RegimentTypes){
				SetupButton(regimentType, rowPosition);
				rowPosition += positionOffset;
			}
			navy.anchoredPosition = rowPosition;
			positionOffset = new Vector2(0, -navy.rect.height);
			rowPosition += positionOffset;
			foreach (ShipType shipType in Player.ShipTypes){
				SetupButton(shipType, rowPosition);
				rowPosition += positionOffset;
			}
		}
		private void SetupButton<T>(UnitType<T> unitType, Vector2 rowPosition) where T : Branch {
			MilitaryUnitButton button = Instantiate(militaryUnitButton, rows);
			button.RectTransform.anchoredPosition = rowPosition;
			button.Button.onClick.AddListener(() => SelectButton(button, unitType));
			button.Label.text = unitType.name;
		}
		private void SelectButton<T>(MilitaryUnitButton button, UnitType<T> unitType) where T : Branch {
			if (selectedButton != null){
				selectedButton.Button.interactable = true;
			}
			selectedButton = button;
			selectedButton.Button.interactable = false;
			selectedRegimentType = unitType as RegimentType;
			selectedShipType = unitType as ShipType;
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
