using UnityEngine;

namespace Player {
	public class EconomyMenu : UILayer, IClosableWindow {
		private bool isDone;
		
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			return LayerBelow.OnSelectableClicked(clickedSelectable, isRightClick);
		}	
		public override bool IsDone(){
			base.IsDone();
			return isDone || Player == null;
		}
		public void Close(){
			isDone = true;
		}
	}
}
