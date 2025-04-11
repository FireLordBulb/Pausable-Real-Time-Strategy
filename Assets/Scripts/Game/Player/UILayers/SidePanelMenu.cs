namespace Player {
	public abstract class SidePanelMenu : UILayer, IRefreshable, IClosableWindow {
		private bool isDone;
		
		public abstract void Refresh();
		public override bool IsDone(){
			base.IsDone();
			return isDone || Player == null;
		}
		public void Close(){
			isDone = true;
		}
	}
}
