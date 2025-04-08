namespace Simulation {
	public class DiplomaticStatus {
		public bool IsAtWar {get; private set;}
		public int TruceDaysLeft {get; private set;}
		public bool CanDeclareWar(){
			return !IsAtWar && TruceDaysLeft <= 0;
		}
		public void DeclareWar(){
			if (CanDeclareWar()){
				IsAtWar = true;
			}
		}
		internal void EndWar(int truceLength){
			IsAtWar = false;
			TruceDaysLeft = truceLength;
			Calendar.Instance.OnDayTick.AddListener(TickTruce, GetType());
		}
		private void TickTruce(){
			TruceDaysLeft--;
			if (0 < TruceDaysLeft){
				return;
			}
			TruceDaysLeft = 0;
			Calendar.Instance.OnDayTick.RemoveListener(TickTruce);
		}
	}
}