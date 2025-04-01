namespace Simulation.Military {
	public class NavyUnit : Unit<Navy> {
		private void Awake(){
			Branch = new Navy();
		}
	}
}