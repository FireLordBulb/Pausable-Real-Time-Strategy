namespace Simulation.Military {
	public class Fleet : Unit<Navy> {
		private void Awake(){
			Branch = new Navy();
		}
	}
}