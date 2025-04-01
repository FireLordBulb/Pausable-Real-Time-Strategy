using Mathematics;
using Simulation;
using Simulation.Military;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class RegimentWindow : UILayer, IRefreshable {
		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI ownerName;
		[SerializeField] private Image ownerFlag;
		[SerializeField] private Button close;
		
		private Regiment regiment;
		
		private void Awake(){
			regiment = UI.SelectedRegiment;
			title.text = $"{regiment.Type.name}";
			ownerName.text = regiment.Owner.Name;
			ownerName.ForceMeshUpdate();
			VectorGeometry.SetRectWidth((RectTransform)ownerName.transform, ownerName.textBounds.size.x);
			AddCountryLink(ownerName.gameObject, regiment.Owner);
			ownerFlag.material = new Material(ownerFlag.material){
				color = regiment.Owner.MapColor
			};
			AddCountryLink(ownerFlag.gameObject, regiment.Owner);
			Refresh();
			close.onClick.AddListener(() => UI.Deselect());
		}
		public void Refresh(){}
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			if (!isRightClick){
				return RegularProvinceClick(clickedSelectable, false);
			}
			switch(clickedSelectable){
				case Province province:
					Player.TryMoveRegimentTo(regiment, province);
					return regiment;
				case Regiment clickedRegiment when regiment != clickedRegiment:
					Player.TryMoveRegimentTo(regiment, clickedRegiment.Location.Province);
					return regiment;
				default:
					return null;
			}
		}
		public override bool IsDone(){
			base.IsDone();
			return UI.SelectedRegiment != regiment;
		}
	}
}
