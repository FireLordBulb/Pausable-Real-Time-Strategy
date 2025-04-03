using Simulation;
using Simulation.Military;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class RegimentWindow : UILayer, IRefreshable {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI action;
		[SerializeField] private TextMeshProUGUI days;
		[SerializeField] private GameObject daysLeftText;
		[SerializeField] private TextMeshProUGUI destination;
		[SerializeField] private TextMeshProUGUI message;
		[SerializeField] private TextMeshProUGUI ownerName;
		[SerializeField] private Image ownerFlag;
		[SerializeField] private Button close;
		
		private Regiment regiment;
		
		private void Awake(){
			regiment = UI.SelectedRegiment;
			title.text = $"{regiment.Type.name}";
			ownerName.text = regiment.Owner.Name;
			SetCountryLink(ownerName, regiment.Owner);
			ownerFlag.material = new Material(ownerFlag.material){
				color = regiment.Owner.MapColor
			};
			SetCountryLink(ownerFlag, regiment.Owner);
			Refresh();
			message.text = "";
			close.onClick.AddListener(() => UI.Deselect());
			Calendar.Instance.OnDayTick.AddListener(Refresh);
		}
		public void Refresh(){
			if (regiment.IsBuilt && regiment.IsMoving){
				action.text = $"Moving to {regiment.NextLocation}";
				days.text = regiment.DaysToNextLocation.ToString();
				daysLeftText.SetActive(true);
				destination.text = regiment.NextLocation == regiment.TargetLocation ? "" : $"End destination: {regiment.TargetLocation}";
			} else if (regiment.IsBuilt && !regiment.IsMoving){
				action.text = $"Located at {regiment.Location}";
				days.text = "";
				daysLeftText.SetActive(false);
				destination.text = "";
			} else {
				action.text = regiment.CreatingVerb;
				days.text = regiment.BuildDaysLeft.ToString();
				daysLeftText.SetActive(true);
				destination.text = "";
			}
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
		
		public override void OnEnd(){
			Calendar.Instance.OnDayTick.RemoveListener(Refresh);
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.SelectedRegiment != regiment;
		}
	}
}
