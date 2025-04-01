namespace Simulation.Military {
	public class ArmyUnit : Unit<Army> {
		private void Awake(){
			Branch = new Army();
		}
	}
}