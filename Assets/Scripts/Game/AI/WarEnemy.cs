using System.Collections.Generic;
using Simulation;

namespace AI {
	public class WarEnemy {
		public readonly Country Country;
		public readonly List<Land> ClosestProvinces;
		private int monthsOfWar;

		public int MonthsOfWar => monthsOfWar;
			
		public WarEnemy(Country country){
			Country = country;
			ClosestProvinces = new List<Land>();
			monthsOfWar = 0;
		}

		public void TickMonth(){
			monthsOfWar++;
		}
	}
}