using ActionStackSystem;
using Simulation;

namespace Player {
	public class UILayer : ActionBehaviour {
		protected static UIStack UI => UIStack.Instance;
		protected static Country Player => UIStack.Instance.PlayerCountry;

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
