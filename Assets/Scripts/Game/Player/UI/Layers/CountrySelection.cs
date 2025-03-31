using Simulation;
using UnityEngine;

namespace Player {
	public class CountrySelection : UILayer {
		[SerializeField] private bool doAutoSelect;
		[SerializeField] private string autoSelectedCountryName;
		
		public override void OnBegin(bool isFirstTime){
#if UNITY_EDITOR
			if (doAutoSelect){
				UI.PlayAs(Country.Get(autoSelectedCountryName));
			}
#endif
		}
		public override Component OnProvinceClicked(Province clickedProvince, bool isRightClick){
			return clickedProvince.Owner == UI.SelectedCountry ? null : clickedProvince.Owner;
		}
		public override bool IsDone(){
			base.IsDone();
			return Player != null;
		}
	}
}
