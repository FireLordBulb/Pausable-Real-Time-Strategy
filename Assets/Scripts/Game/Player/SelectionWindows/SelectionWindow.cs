using System;
using System.Diagnostics;
using Simulation;

namespace Player {
	public abstract class SelectionWindow<TSelectable> : SelectionWindow where TSelectable : class, ISelectable {
		protected TSelectable Selected;
		
		internal override Type TargetType => typeof(TSelectable);
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			Selected = UI.Selected as TSelectable;
			Debug.Assert(Selected != null, $"SelectionWindow {gameObject.name} was instantiated without an object of its TargetType ({TargetType}) being selected!");
			Selected.OnSelect();
		}
		public override void OnEnd(){
			if (Selected is not UnityEngine.Object unityObject || unityObject != null){
				Selected.OnDeselect();
			}
			base.OnEnd();
		}
		public override bool IsDone(){
			base.IsDone();
			return !ReferenceEquals(UI.Selected, Selected) || Selected is UnityEngine.Object unityObject && unityObject == null;
		}
		public override void Close(){
			UI.Deselect(Selected);
		}
	}
	public abstract class SelectionWindow : UILayer, IRefreshable, IClosableWindow {
		internal abstract Type TargetType {get;}
		
		public abstract void Refresh();
		public abstract void Close();
	}
}
