using Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class CountrySelection : UILayer {
		[SerializeField] private Button observe;
		[SerializeField] private bool doAutoSelect;
		[SerializeField] private bool isObserver;
		[SerializeField] private string autoSelectedCountryName;

		private bool isDone;
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
#if UNITY_EDITOR
			if (doAutoSelect){
				UI.PlayAs(isObserver ? null : UI.Map.GetCountry(autoSelectedCountryName));
				isDone = true;
			}
#endif
			observe.onClick.AddListener(() => {
				isDone = true;
			});
		}
		public override ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			if (clickedSelectable is not Province clickedProvince){
				return null;
			}
			return clickedProvince.IsSea ? null : clickedProvince.Land.Owner == UI.SelectedCountry ? null : clickedProvince.Land.Owner;
		}
		public override bool IsDone(){
			base.IsDone();
			return Player != null || isDone;
		}
	}
}
