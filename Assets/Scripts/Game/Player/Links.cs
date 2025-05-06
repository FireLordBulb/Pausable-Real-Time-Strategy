using System;
using System.Collections.Generic;
using Mathematics;
using Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Player {
	public class Links {
		private readonly Dictionary<GameObject, (UILink link, ISelectable selectable)> existingLinks = new();
		private readonly Action<ISelectable, bool> select;
		
		public Links(Action<ISelectable, bool> selectAction){
			select = selectAction;
		}
		
		public void Add(TextMeshProUGUI linkText, ISelectable selectable, bool doDeselect = false, Action action = null){
			if (existingLinks.TryGetValue(linkText.gameObject, out (UILink, ISelectable selectable) tuple) && tuple.selectable == selectable){
				return;
			}
			linkText.ForceMeshUpdate();
			RectTransform rectTransform = (RectTransform)linkText.transform;
			VectorGeometry.SetRectWidth(rectTransform, linkText.textBounds.size.x);
			Add(rectTransform, selectable, doDeselect, action);
		}
		// ReSharper disable Unity.PerformanceAnalysis // The expensive AddComponent won't be called every frame because existingLinks keeps track of it there already is a link.
		public void Add(Component linkComponent, ISelectable selectable, bool doDeselect = false, Action action = null){
			if (existingLinks.TryGetValue(linkComponent.gameObject, out (UILink link, ISelectable selectable) tuple)){
				if (tuple.selectable == selectable){
					return;
				}
				existingLinks.Remove(linkComponent.gameObject);
				Object.DestroyImmediate(tuple.link);
			}
			UILink link = linkComponent.gameObject.AddComponent<UILink>();
			link.Link(action == null ? 
				() => select(selectable, doDeselect) :
				() => {
					select(selectable, doDeselect);
					action();
				}
			);
			existingLinks.Add(linkComponent.gameObject, (link, selectable));
		}

		public void Remove(Component linkComponent){
			if (!existingLinks.TryGetValue(linkComponent.gameObject, out (UILink link, ISelectable) tuple)){
				return;
			}
			existingLinks.Remove(linkComponent.gameObject);
			Object.DestroyImmediate(tuple.link);
		}
		
		public void LinkButton(Button button, ISelectable selectable, bool doDeselect = false){
			button.onClick.AddListener(() => select(selectable, doDeselect));
		}
	}
}
