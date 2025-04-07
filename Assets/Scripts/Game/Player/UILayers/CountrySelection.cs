using Simulation;
using UnityEngine;

namespace Player {
	public class CountrySelection : UILayer {
		[SerializeField] private bool doAutoSelect;
		[SerializeField] private bool isObserver;
		[SerializeField] private string autoSelectedCountryName;
		
		// ReSharper disable Unity.PerformanceAnalysis // This is editor only AND OnBegin isn't called even near every frame.
		public override void OnBegin(bool isFirstTime){
#if UNITY_EDITOR
			if (doAutoSelect){
				UI.PlayAs(isObserver ? null : Country.Get(autoSelectedCountryName));
			}
#endif
		}
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			if (clickedSelectable is not Province clickedProvince){
				return null;
			}
			return clickedProvince.IsLand && clickedProvince.Land.Owner == UI.SelectedCountry ? null : clickedProvince.Land.Owner;
		}
		public override void OnEnd(){
			UI.Deselect();
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
#if UNITY_EDITOR
			return Player != null || doAutoSelect;
#else
			return Player != null;
#endif
		}
	}
}
