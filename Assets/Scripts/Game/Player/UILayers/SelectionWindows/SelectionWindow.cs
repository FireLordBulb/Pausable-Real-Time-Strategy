using System;
using Simulation;

namespace Player {
	public abstract class SelectionWindow<TSelectable> : SelectionWindow where TSelectable : class, ISelectable {
		protected TSelectable Selected;
		
		internal override Type TargetType => typeof(TSelectable);
		
		internal override void Init(UIStack uiStack){
			base.Init(uiStack);
			Selected = UI.Selected as TSelectable;
		}
	}
	public abstract class SelectionWindow : UILayer, IRefreshable, IClosableWindow {
		internal abstract Type TargetType {get;}
		
		public abstract void Refresh();
		public abstract void Close();
	}
}
