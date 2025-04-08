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

		public virtual ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick) => null;

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
		private static readonly Dictionary<GameObject, (UILink link, ISelectable selectable)> Links = new();
		public static void SetSelectLink(TextMeshProUGUI linkText, ISelectable selectable){
			if (Links.TryGetValue(linkText.gameObject, out (UILink, ISelectable selectable) tuple) && tuple.selectable == selectable){
				return;
			}
			linkText.ForceMeshUpdate();
			RectTransform rectTransform = (RectTransform)linkText.transform;
			VectorGeometry.SetRectWidth(rectTransform, linkText.textBounds.size.x);
			SetSelectLink(rectTransform, selectable);
		}
		public static void SetSelectLink(Component linkComponent, ISelectable selectable){
			if (Links.TryGetValue(linkComponent.gameObject, out (UILink link, ISelectable selectable) tuple)){
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
		
		public static Simulation.Military.Harbor GetHarbor(Province province){
			if (!province.IsCoast){
				return null;
			} 
			Simulation.Military.Harbor closestHarbor = null;
			float closestSquareDistance = float.MaxValue;
			foreach (ProvinceLink provinceLink in province.Links){
				if (provinceLink is not ShallowsLink shallowsLink){
					continue;
				}
				Simulation.Military.Harbor harbor = shallowsLink.Harbor;
				float squareDistance = (harbor.WorldPosition-UI.MouseWorldPosition).sqrMagnitude;
				if (closestSquareDistance < squareDistance){
					continue;
				}
				closestHarbor = harbor;
				closestSquareDistance = squareDistance;
			}
			// All coastal provinces have at least one harbor.
			Debug.Assert(closestHarbor != null);
			return closestHarbor;
		}
	}
}
