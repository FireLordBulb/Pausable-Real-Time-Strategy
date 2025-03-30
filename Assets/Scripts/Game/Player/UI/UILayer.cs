using ActionStackSystem;
using Simulation;
using UnityEngine;

namespace Player {
	public abstract class UILayer : ActionBehaviour {
		protected static UIStack UI => UIStack.Instance;
		protected static Country Player => UIStack.Instance.PlayerCountry;

		public override void OnBegin(bool isFirstTime){}

		public virtual Component OnProvinceClicked(Province clickedProvince, bool isRightClick) => null;

		public override void OnEnd(){
			Destroy(gameObject);
		}
		public override bool IsDone(){
			return false;
		}
		
		// Subclass Sandbox. |>-------------------------------------------------------------------------------------------
		protected static Component RegularProvinceClick(Province clickedProvince, bool isRightClick){
			if (isRightClick){
				return clickedProvince.Owner == UI.SelectedCountry ? null : clickedProvince.Owner;
			}
			return clickedProvince == UI.SelectedProvince ? null : clickedProvince;
		}

		protected static void AddCountryLink(GameObject linkObject, Country country){
			linkObject.AddComponent<UILink>().Link(() => UI.Select(country));
		}
	}
}
