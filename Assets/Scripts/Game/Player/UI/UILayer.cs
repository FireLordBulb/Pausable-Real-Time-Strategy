using ActionStackSystem;

namespace Player {
	public class UILayer : ActionBehaviour {
		protected static UIStack UI => UIStack.Instance;

		public override void OnBegin(bool isFirstTime){}
		public virtual void OnProvinceSelected(){}
		public override void OnEnd(){
			Destroy(gameObject);
		}
		public override bool IsDone(){
			return false;
		}
	}
}
