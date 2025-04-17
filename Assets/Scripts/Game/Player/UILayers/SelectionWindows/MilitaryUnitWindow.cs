using Simulation;
using Simulation.Military;
using TMPro;
using UnityEngine;

namespace Player {
	public abstract class MilitaryUnitWindow<TUnit> : SelectionWindow<TUnit> where TUnit : Unit<TUnit> {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI action;
		[SerializeField] private TextMeshProUGUI location;
		[SerializeField] private TextMeshProUGUI days;
		[SerializeField] private GameObject daysLeftText;
		[SerializeField] private GameObject destination;
		[SerializeField] private TextMeshProUGUI destinationLocation;
		[SerializeField] private TextMeshProUGUI message;
		[SerializeField] private CountryPanel countryPanel;
		[SerializeField] private GameObject shiftTip;
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			title.text = $"{Selected.Type.name}";
			countryPanel.SetCountry(Selected.Owner, UI);
			Refresh();
			message.text = "";
			Calendar.OnDayTick.AddListener(Refresh);
		}
		public override void Refresh(){
			if (Selected.IsBuilt && Selected.IsMoving){
				SetLeftOfLinkText("Moving to ", action, location);
				location.text = Selected.NextLocation.Name;
				UI.Links.Add(location, Selected.NextLocation.Province);
				days.text = Selected.DaysToNextLocation.ToString();
				daysLeftText.SetActive(true);
				if (Selected.NextLocation == Selected.TargetLocation){
					destination.SetActive(false);
				} else {
					destination.SetActive(true);
					destinationLocation.text = Selected.TargetLocation.Name;
					UI.Links.Add(destinationLocation, Selected.TargetLocation.Province);
				}
			} else if (Selected.IsBuilt && !Selected.IsMoving){
				SetLeftOfLinkText("Located at ", action, location);
				location.text = Selected.Location.Name;
				UI.Links.Add(location, Selected.Location.Province);
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
			shiftTip.SetActive(Selected.Owner == Player);
		}
		private void SetLeftOfLinkText(string newText, TMP_Text textMesh, TMP_Text link){
			if (textMesh.text == newText){
				return;
			}
			textMesh.text = newText;
			textMesh.ForceMeshUpdate();
			float x = textMesh.textInfo.characterInfo[textMesh.textInfo.lineInfo[0].lastCharacterIndex].xAdvance;
			((RectTransform)link.transform).anchoredPosition = new Vector2(x, 0);
		}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			if (!isRightClick){
				return LayerBelow.OnSelectableClicked(clickedSelectable, false);
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
		private void MoveTo(Province province){
			if (Player == null){
				return;
			}
			OrderMove(province);
			Refresh();
		}
		protected abstract void OrderMove(Province province);
		protected void SetMessage(string text){
			message.text = text;
		}
		
		public override void OnEnd(){
			Calendar.OnDayTick.RemoveListener(Refresh);
			base.OnEnd();
		}
	}
}
