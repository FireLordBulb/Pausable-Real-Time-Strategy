namespace Simulation.Military {
	public class Regiment : Unit<Army> {
		private void Awake(){
			Branch = new Army();
		}
	}
}