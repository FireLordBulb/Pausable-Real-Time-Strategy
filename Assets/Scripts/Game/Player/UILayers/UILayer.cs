using ActionStackSystem;
using Mathematics;
using Simulation;
using TMPro;
using UnityEngine;

namespace Player {
	public abstract class UILayer : ActionBehaviour {
		private UILayer layerBelow;
		
		protected UILayer LayerBelow {get {
			if (layerBelow == null){
				layerBelow = UI.GetLayerBelow(this);
			}
			return layerBelow;
		}}
		protected static UIStack UI => UIStack.Instance;
		protected static Country Player => UI.PlayerCountry;
		
		public override void OnBegin(bool isFirstTime){}

		public virtual Component OnSelectableClicked(Component clickedSelectable, bool isRightClick) => null;

		public override void OnEnd(){
			if (this != null){
				Destroy(gameObject);
			}
		}
		public override bool IsDone(){
			if (LayerBelow != null && LayerBelow.IsDone()){
				LayerBelow.OnEnd();
			}
			return false;
		}
		
		// Subclass Sandbox. |>-------------------------------------------------------------------------------------------
		protected static Component RegularProvinceClick(Component clickedSelectable, bool isRightClick){
			if (clickedSelectable is not Province clickedProvince){
				return clickedSelectable == UI.Selected ? null : clickedSelectable;
			}
			if (isRightClick){
				return clickedProvince.Owner == UI.SelectedCountry ? null : clickedProvince.Owner;
			}
			return clickedProvince == UI.SelectedProvince ? null : clickedProvince;
		}

		protected static void SetCountryLink(TextMeshProUGUI linkText, Country country){
			linkText.ForceMeshUpdate();
			RectTransform rectTransform = (RectTransform)linkText.transform;
			VectorGeometry.SetRectWidth(rectTransform, linkText.textBounds.size.x);
			SetCountryLink(rectTransform, country);
		}
		protected static void SetCountryLink(Component linkComponent, Country country){
			DestroyImmediate(linkComponent.GetComponent<UILink>());
			linkComponent.gameObject.AddComponent<UILink>().Link(() => UI.Select(country));
		}
		
	}
}
