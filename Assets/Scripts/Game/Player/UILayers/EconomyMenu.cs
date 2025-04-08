namespace Player {
	public class EconomyMenu : UILayer, IRefreshable, IClosableWindow {
		private bool isDone;
		
		public void Refresh(){}
		public override bool IsDone(){
			base.IsDone();
			return isDone || Player == null;
		}
		public void Close(){
			isDone = true;
		}
	}
}
