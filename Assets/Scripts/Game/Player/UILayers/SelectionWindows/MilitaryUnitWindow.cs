using Mathematics;
using Simulation;
using Simulation.Military;
using TMPro;
using UnityEngine;

namespace Player {
	public abstract class MilitaryUnitWindow<TUnit> : SelectionWindow<TUnit> where TUnit : class, IUnit {
		[SerializeField] private TextMeshProUGUI title;
		[Header("Combat")]
		[SerializeField] protected ValueTable combatValuesTable;
		[SerializeField] private string[] combatValueNames;
		[Header("Movement")]
		[SerializeField] private TextMeshProUGUI action;
		[SerializeField] private TextMeshProUGUI location;
		[SerializeField] private float locationMaxWidthMoving;
		[SerializeField] private float locationMaxWidthIdle;
		[SerializeField] private float destinationLocationMaxWidth;
		[SerializeField] protected TextMeshProUGUI days;
		[SerializeField] protected GameObject daysLeftText;
		[SerializeField] private GameObject destination;
		[SerializeField] private TextMeshProUGUI destinationLocation;
		[SerializeField] private TextMeshProUGUI message;
		[Space]
		[SerializeField] private CountryPanel countryPanel;
		[SerializeField] private GameObject shiftTip;
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			combatValuesTable.Generate(0, combatValueNames);
			title.text = $"{Selected.TypeName}";
			countryPanel.SetCountry(Selected.Owner, UI);
			Refresh();
			message.text = "";
			Calendar.OnDayTick.AddListener(Refresh);
		}
		public override void Refresh(){
			RefreshCombatTable();
			location.fontSize = location.fontSizeMax;
			destinationLocation.fontSize = destinationLocation.fontSizeMax;
			if (Selected.IsBuilt && Selected.IsMoving){
				SetLeftOfLinkText(Selected.IsRetreating ? "Retreating to " : "Moving to ");
				location.text = Selected.NextLocationInterface.Name;
				UI.Links.Add(location, Selected.NextLocationInterface.Province);
				days.text = Selected.DaysToNextLocation.ToString();
				daysLeftText.SetActive(true);
				if (Selected.NextLocationInterface == Selected.TargetLocationInterface){
					destination.SetActive(false);
				} else {
					destination.SetActive(true);
					destinationLocation.text = Selected.TargetLocationInterface.Name;
					UI.Links.Add(destinationLocation, Selected.TargetLocationInterface.Province);
				}
			} else if (Selected.IsBuilt && !Selected.IsMoving){
				SetLeftOfLinkText(Selected.LocationInterface.IsBattleOngoing ? "In combat at " : "Located at ");
				location.text = Selected.LocationInterface.Name;
				UI.Links.Add(location, Selected.LocationInterface.Province);
				days.text = "";
				daysLeftText.SetActive(false);
				destination.SetActive(false);
			} else {
				action.text = Selected.CreatingVerb;
				location.text = "";
				days.text = Selected.BuildDaysLeft.ToString();
				daysLeftText.SetActive(true);
				destination.SetActive(false);
			}
			CapWidth(location, Selected.IsMoving ? locationMaxWidthMoving : locationMaxWidthIdle);
			CapWidth(destinationLocation, destinationLocationMaxWidth);
			shiftTip.SetActive(Selected.Owner == Player);
		}
		// Manually resize because the built-in autoSize doesn't work properly together with UI.Links.Add().
		private static void CapWidth(TMP_Text text, float maxWidth){
			text.ForceMeshUpdate();
			while (text.textBounds.size.x > maxWidth && text.fontSize > text.fontSizeMin){
				text.fontSize--;
				text.ForceMeshUpdate();
			}
			VectorGeometry.SetRectWidth(text.rectTransform, Mathf.Min(text.textBounds.size.x, maxWidth));
		}
		protected abstract void RefreshCombatTable();
		protected void SetLeftOfLinkText(string newText){
			if (action.text == newText){
				return;
			}
			action.text = newText;
			action.ForceMeshUpdate();
			float x = action.textInfo.characterInfo[action.textInfo.lineInfo[0].lastCharacterIndex].xAdvance;
			((RectTransform)location.transform).anchoredPosition = new Vector2(x, 0);
		}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			if (!isRightClick){
				return LayerBelow.OnSelectableClicked(clickedSelectable, false);
			}
			if (Player == null){
				return Selected;
			}
			if (DoBypassDefaultBehaviour(clickedSelectable)){
				Refresh();
				return Selected;
			}
			switch(clickedSelectable){
				case Province province:
					MoveTo(province);
					return Selected;
				case IUnit clickedUnit when !ReferenceEquals(Selected, clickedUnit):
					MoveTo(clickedUnit.Province);
					return Selected;
				default:
					return null;
			}
		}
		protected virtual bool DoBypassDefaultBehaviour(ISelectable clickedSelectable){
			return false;
		}
		private void MoveTo(Province province){
			OrderMove(province);
			Refresh();
		}
		protected abstract void OrderMove(Province province);
		internal void SetMessage(string text){
			message.text = text;
		}
		
		public override void OnEnd(){
			Calendar.OnDayTick.RemoveListener(Refresh);
			base.OnEnd();
		}
	}
}
