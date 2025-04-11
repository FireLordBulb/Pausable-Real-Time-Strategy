using ActionStackSystem;
using Simulation;

namespace Player {
	public abstract class UILayer : ActionBehaviour {
		private UILayer layerBelow;
		
		protected UIStack UI {get; private set;}
		protected UILayer LayerBelow {get {
			if (layerBelow == null){
				layerBelow = UI.GetLayerBelow(this);
			}
			return layerBelow;
		}}
		protected Country Player => UI.PlayerCountry;
		protected Calendar Calendar => UI.Map.Calendar;

		internal void Init(UIStack uiStack){
			UI = uiStack;
		}
		
		public override void OnBegin(bool isFirstTime){}
		
		public virtual ISelectable OnSelectableClicked(ISelectable clickedSelectable, bool isRightClick){
			return LayerBelow.OnSelectableClicked(clickedSelectable, isRightClick);
		}
		public virtual void OnDrag(bool isRightClick){
			UI.Deselect();
		}
		
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
	}
}
