using Simulation;
using Simulation.Military;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class RegimentWindow : UILayer, IRefreshable, IClosableWindow {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI action;
		[SerializeField] private TextMeshProUGUI location;
		[SerializeField] private TextMeshProUGUI days;
		[SerializeField] private GameObject daysLeftText;
		[SerializeField] private GameObject destination;
		[SerializeField] private TextMeshProUGUI destinationLocation;
		[SerializeField] private TextMeshProUGUI message;
		[SerializeField] private TextMeshProUGUI ownerName;
		[SerializeField] private Image ownerFlag;
		
		private Regiment regiment;
		
		private void Awake(){
			regiment = UI.SelectedRegiment;
			title.text = $"{regiment.Type.name}";
			ownerName.text = regiment.Owner.Name;
			SetSelectLink(ownerName, regiment.Owner);
			ownerFlag.material = new Material(ownerFlag.material){
				color = regiment.Owner.MapColor
			};
			SetSelectLink(ownerFlag, regiment.Owner);
			Refresh();
			message.text = "";
			Calendar.Instance.OnDayTick.AddListener(Refresh);
		}
		public void Refresh(){
			if (regiment.IsBuilt && regiment.IsMoving){
				SetLeftOfLinkText("Moving to ", action, location);
				location.text = regiment.NextLocation.Name;
				SetSelectLink(location, regiment.NextLocation.Province);
				days.text = regiment.DaysToNextLocation.ToString();
				daysLeftText.SetActive(true);
				if (regiment.NextLocation == regiment.TargetLocation){
					destination.SetActive(false);
				} else {
					destinationLocation.text = regiment.TargetLocation.Name;
					SetSelectLink(destinationLocation, regiment.TargetLocation.Province);
					destination.SetActive(true);
				}
			} else if (regiment.IsBuilt && !regiment.IsMoving){
				SetLeftOfLinkText("Located at ", action, location);
				location.text = regiment.Location.Name;
				SetSelectLink(location, regiment.Location.Province);
				days.text = "";
				daysLeftText.SetActive(false);
				destination.SetActive(false);
			} else {
				action.text = regiment.CreatingVerb;
				location.text = "";
				days.text = regiment.BuildDaysLeft.ToString();
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
			//textMesh.textInfo.lineInfo[0].lastCharacterIndex
			float x = textMesh.textInfo.characterInfo[textMesh.textInfo.lineInfo[0].lastCharacterIndex].xAdvance;
			((RectTransform)link.transform).anchoredPosition = new Vector2(x, 0);
		}
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			if (!isRightClick){
				return RegularProvinceClick(clickedSelectable, false);
			}
			switch(clickedSelectable){
				case Province province:
					MoveTo(province);
					return regiment;
				case Regiment clickedRegiment when regiment != clickedRegiment:
					MoveTo(clickedRegiment.Location.Province);
					return regiment;
				default:
					return null;
			}
		}
		private void MoveTo(Province province){
			if (Player == null){
				return;
			}
			MoveOrderResult result = Player.MoveRegimentTo(regiment, province);
			message.text = result switch {
				MoveOrderResult.NotBuilt => "Cannot move an army before it has finished recruiting!",
				MoveOrderResult.NoPath => $"Cannot move to {province} because the landmass is separated by sea!",
				MoveOrderResult.NoAccess => "You cannot cross another country's borders when you're at peace with it!",
				MoveOrderResult.InvalidTarget => "Armies cannot walk on water!",
				MoveOrderResult.NotOwner => "You cannot move another country's units!",
				_ => ""
			};
			Refresh();
		}
		
		// ReSharper disable Unity.PerformanceAnalysis // This doesn't ever call the performance-intensive Refresh, it only removes it as a listener. 
		public override void OnEnd(){
			Calendar.Instance.OnDayTick.RemoveListener(Refresh);
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.SelectedRegiment != regiment;
		}
		public void Close(){
			UI.Deselect();
		}
	}
}
