using System.Collections.Generic;
using Simulation;

namespace AI {
	internal class WarEnemy {
		public readonly Country Country;
		public readonly List<Land> ClosestProvinces;
		public bool HasOverseasLand;
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