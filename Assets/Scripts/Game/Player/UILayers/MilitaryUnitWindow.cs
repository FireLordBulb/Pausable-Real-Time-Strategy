using Simulation;
using Simulation.Military;
using TMPro;
using UnityEngine;

namespace Player {
	public abstract class MilitaryUnitWindow<TUnit> : UILayer, IRefreshable, IClosableWindow where TUnit : Unit<TUnit> {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI action;
		[SerializeField] private TextMeshProUGUI location;
		[SerializeField] private TextMeshProUGUI days;
		[SerializeField] private GameObject daysLeftText;
		[SerializeField] private GameObject destination;
		[SerializeField] private TextMeshProUGUI destinationLocation;
		[SerializeField] private TextMeshProUGUI message;
		[SerializeField] private CountryPanel countryPanel;

		protected TUnit Unit;
		
		private void Awake(){
			Unit = (TUnit)UI.Selected;
			title.text = $"{Unit.Type.name}";
			countryPanel.SetCountry(Unit.Owner);
			Refresh();
			message.text = "";
			Calendar.Instance.OnDayTick.AddListener(Refresh);
		}
		public void Refresh(){
			if (Unit.IsBuilt && Unit.IsMoving){
				SetLeftOfLinkText("Moving to ", action, location);
				location.text = Unit.NextLocation.Name;
				SetSelectLink(location, Unit.NextLocation.Province);
				days.text = Unit.DaysToNextLocation.ToString();
				daysLeftText.SetActive(true);
				if (Unit.NextLocation == Unit.TargetLocation){
					destination.SetActive(false);
				} else {
					destination.SetActive(true);
					destinationLocation.text = Unit.TargetLocation.Name;
					SetSelectLink(destinationLocation, Unit.TargetLocation.Province);
				}
			} else if (Unit.IsBuilt && !Unit.IsMoving){
				SetLeftOfLinkText("Located at ", action, location);
				location.text = Unit.Location.Name;
				SetSelectLink(location, Unit.Location.Province);
				days.text = "";
				daysLeftText.SetActive(false);
				destination.SetActive(false);
			} else {
				action.text = Unit.CreatingVerb;
				location.text = "";
				days.text = Unit.BuildDaysLeft.ToString();
				daysLeftText.SetActive(true);
				destination.SetActive(false);
			}
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
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			if (!isRightClick){
				return LayerBelow.OnSelectableClicked(clickedSelectable, false);
			}
			switch(clickedSelectable){
				case Province province:
					MoveTo(province);
					return Unit;
				case TUnit clickedUnit when Unit != clickedUnit:
					MoveTo(clickedUnit.Location.Province);
					return Unit;
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
		
		// ReSharper disable Unity.PerformanceAnalysis // This doesn't ever call the performance-intensive Refresh, it only removes it as a listener. 
		public override void OnEnd(){
			Calendar.Instance.OnDayTick.RemoveListener(Refresh);
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.Selected != Unit;
		}
		public void Close(){
			UI.Deselect();
		}
	}
}
