namespace Simulation.Military {
	public class Regiment : Unit<Army> {
		private void Awake(){
			Branch = new Army();
		}
		protected override Location<Army> GetLocation(Province province){
			return province.Land.ArmyLocation;
		}
	}
}