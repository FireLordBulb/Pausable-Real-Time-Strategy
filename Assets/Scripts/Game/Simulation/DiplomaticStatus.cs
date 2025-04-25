namespace Simulation {
	public class DiplomaticStatus {
		private readonly Calendar calendar;
		
		public bool IsAtWar {get; private set;}
		public int TruceDaysLeft {get; private set;}

		public DiplomaticStatus(Calendar calendarReference){
			calendar = calendarReference;
		}
		
		public bool CanDeclareWar(Country target){
			return !IsAtWar && TruceDaysLeft <= 0 && target.enabled;
		}
		internal void DeclareWar(Country target){
			if (CanDeclareWar(target)){
				IsAtWar = true;
			}
		}
		internal void EndWar(int truceLength){
			IsAtWar = false;
			TruceDaysLeft = truceLength;
			calendar.OnDayTick.AddListener(TickTruce);
		}
		private void TickTruce(){
			TruceDaysLeft--;
			if (0 < TruceDaysLeft){
				return;
			}
			TruceDaysLeft = 0;
			calendar.OnDayTick.RemoveListener(TickTruce);
		}
	}
}