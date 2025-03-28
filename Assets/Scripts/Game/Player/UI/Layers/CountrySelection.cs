using Simulation;
using UnityEngine;

namespace Player {
	public class CountrySelection : UILayer {
		[SerializeField] private bool doAutoSelect;
		[SerializeField] private string autoSelectedCountryName;
		
		public override void OnBegin(bool isFirstTime){
#if UNITY_EDITOR
			if (doAutoSelect){
				UI.PlayerCountry = Country.Get(autoSelectedCountryName);
			}
#endif
		}
		public override void OnProvinceSelected(){
			if (UI.SelectedProvince.HasOwner){
				UI.PlayerCountry = UI.SelectedProvince.Owner;
			}
		}
		public override void OnEnd(){
			print(UI.PlayerCountry.name);
			UI.DeselectProvince();
			base.OnEnd();
		}
		public override bool IsDone(){
			return UI.PlayerCountry != null;
		}
	}
}
