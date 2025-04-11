using Simulation;
using Simulation.Military;
using UnityEngine;

namespace Player {
	public class WarMenu : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private MilitaryUnitButton militaryUnitButton;
		[SerializeField] private RectTransform army;
		[SerializeField] private RectTransform navy;

		private bool isDone;
		private MilitaryUnitButton selectedButton;
		private RegimentType selectedRegimentType;
		private ShipType selectedShipType;
		
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
		private RectTransform SetupButton<TUnit>(UnitType<TUnit> unitType, RectTransform parent) where TUnit : Unit<TUnit> {
			MilitaryUnitButton button = Instantiate(militaryUnitButton, parent);
			button.Button.onClick.AddListener(() => SelectUnit(button, unitType));
			button.gameObject.name = unitType.name;
			button.Label.text = unitType.name;
			button.Cost.text += unitType.GetCostAsString();
			return button.RectTransform;
		}
		private void SelectUnit<TUnit>(MilitaryUnitButton button, UnitType<TUnit> unitType) where TUnit : Unit<TUnit> {
			if (selectedButton == button){
				DeselectButton();
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
		}
		
		public void Refresh(){
			if (selectedRegimentType != null){
				Refresh(selectedRegimentType);
			} else if (selectedShipType != null){
				Refresh(selectedShipType);
			}
		}
		private void Refresh<TUnit>(UnitType<TUnit> unitType) where TUnit : Unit<TUnit> {
			selectedButton.Message.text = unitType.CanBeBuiltBy(Player) ? $"<color=green>Can be {unitType.CreatedVerb}</color>" : "<color=red>Cannot afford!</color>";
		}

		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			if (isRightClick || clickedSelectable is not Province clickedProvince){
				DeselectButton();
				return LayerBelow.OnSelectableClicked(clickedSelectable, true);
			}
			if (selectedRegimentType != null){
				Player.TryStartRecruitingRegiment(selectedRegimentType, clickedProvince);
				UI.Refresh();
			} else if (selectedShipType != null){
				Player.TryStartConstructingFleet(selectedShipType, UI.GetHarbor(clickedProvince));
				UI.Refresh();
			} else {
				return clickedProvince;
			}
			return UI.Selected;
		}

		private void DeselectButton(){
			if (selectedButton != null){
				selectedButton.HideInfoBox();
				selectedButton = null;
			}
			selectedRegimentType = null;
			selectedShipType = null;
		}
		
		public override bool IsDone(){
			base.IsDone();
			return isDone || Player == null;
		}
		public void Close(){
			isDone = true;
		}
	}
}
