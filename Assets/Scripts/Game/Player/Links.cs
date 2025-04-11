using System;
using System.Collections.Generic;
using Mathematics;
using Simulation;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Player {
	public class Links {
		private readonly Dictionary<GameObject, (UILink link, ISelectable selectable)> existingLinks = new();
		private readonly Action<ISelectable> select;
		
		public Links(Action<ISelectable> selectAction){
			select = selectAction;
		}
		
		public void Add(TextMeshProUGUI linkText, ISelectable selectable, Action action = null){
			if (existingLinks.TryGetValue(linkText.gameObject, out (UILink, ISelectable selectable) tuple) && tuple.selectable == selectable){
				return;
			}
			linkText.ForceMeshUpdate();
			RectTransform rectTransform = (RectTransform)linkText.transform;
			VectorGeometry.SetRectWidth(rectTransform, linkText.textBounds.size.x);
			Add(rectTransform, selectable, action);
		}
		public void Add(Component linkComponent, ISelectable selectable, Action action = null){
			if (existingLinks.TryGetValue(linkComponent.gameObject, out (UILink link, ISelectable selectable) tuple)){
				if (tuple.selectable == selectable){
					return;
				}
				existingLinks.Remove(linkComponent.gameObject);
				Object.DestroyImmediate(tuple.link);
			}
			UILink link = linkComponent.gameObject.AddComponent<UILink>();
			link.Link(action == null ? 
				() => select(selectable) :
				() => {
					select(selectable);
					action();
				}
			);
			existingLinks.Add(linkComponent.gameObject, (link, selectable));
		}
	}
}
