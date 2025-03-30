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
		public override Component OnProvinceClicked(Province clickedProvince, bool isRightClick){
			return clickedProvince.Owner == UI.SelectedCountry ? null : clickedProvince.Owner;
		}
		public override void OnEnd(){
			UI.Deselect();
			CameraMovement cameraMovement = CameraMovement.Instance;
			cameraMovement.SetZoom(cameraMovement.MaxZoom, cameraMovement.Camera.WorldToScreenPoint(Player.Capital.WorldPosition));
			base.OnEnd();
		}
		public override bool IsDone(){
			return Player != null;
		}
	}
}
