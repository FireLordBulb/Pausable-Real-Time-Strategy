using System.Collections.Generic;
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


		private static readonly Dictionary<GameObject, (UILink link, Component selectable)> Links = new();
		protected static void SetSelectLink(TextMeshProUGUI linkText, Component selectable){
			if (Links.TryGetValue(linkText.gameObject, out (UILink, Component selectable) tuple) && tuple.selectable == selectable){
				return;
			}
			linkText.ForceMeshUpdate();
			RectTransform rectTransform = (RectTransform)linkText.transform;
			VectorGeometry.SetRectWidth(rectTransform, linkText.textBounds.size.x);
			SetSelectLink(rectTransform, selectable);
		}
		protected static void SetSelectLink(Component linkComponent, Component selectable){
			if (Links.TryGetValue(linkComponent.gameObject, out (UILink link, Component selectable) tuple)){
				if (tuple.selectable == selectable){
					return;
				}
				Links.Remove(linkComponent.gameObject);
				DestroyImmediate(tuple.link);
			}
			UILink link = linkComponent.gameObject.AddComponent<UILink>();
			link.Link(() => UI.Select(selectable));
			Links.Add(linkComponent.gameObject, (link, selectable));
		}
	}
}
