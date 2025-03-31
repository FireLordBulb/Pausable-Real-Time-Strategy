using Simulation;
using UnityEngine;

namespace Player {
	public class CountrySelection : UILayer {
		[SerializeField] private bool doAutoSelect;
		[SerializeField] private bool isObserver;
		[SerializeField] private string autoSelectedCountryName;
		
		public override void OnBegin(bool isFirstTime){
#if UNITY_EDITOR
			if (doAutoSelect){
				UI.PlayAs(isObserver ? null : Country.Get(autoSelectedCountryName));
			}
#endif
		}
		public override Component OnProvinceClicked(Province clickedProvince, bool isRightClick){
			return clickedProvince.Owner == UI.SelectedCountry ? null : clickedProvince.Owner;
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
