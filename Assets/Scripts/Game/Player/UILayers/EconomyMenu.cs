using UnityEngine;
using UnityEngine.UI;

namespace Player {
	public class EconomyMenu : UILayer {
		[SerializeField] private Button close;

		private bool isDone;
		
		private void Awake(){
			close.onClick.AddListener(() => isDone = true);
		}
		
		public override Component OnSelectableClicked(Component clickedSelectable, bool isRightClick){
			return LayerBelow.OnSelectableClicked(clickedSelectable, isRightClick);
		}	
		public override bool IsDone(){
			base.IsDone();
			return isDone || Player == null;
		}
	}
}