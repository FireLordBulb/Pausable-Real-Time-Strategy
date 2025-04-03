namespace Simulation.Military {
	public class Ship : Unit<Navy> {
		private void Awake(){
			Branch = new Navy();
		}
		protected override Location<Navy> GetLocation(Province province){
			return province.Sea.NavyLocation;
		}
	}
}