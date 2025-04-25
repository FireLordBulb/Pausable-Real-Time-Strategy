using System.Collections.Generic;
using Simulation;

namespace AI {
	internal class WarEnemy {
		public readonly Country Country;
		public readonly List<Land> ClosestProvinces;
		public readonly List<Land> OverseasProvinces;
		private int monthsOfWar;

		public bool HasOverseasLand => OverseasProvinces.Count > 0;
		public int MonthsOfWar => monthsOfWar;
			
		public WarEnemy(Country country){
			Country = country;
			ClosestProvinces = new List<Land>();
			OverseasProvinces = new List<Land>();
			monthsOfWar = 0;
		}

		public void TickMonth(){
			monthsOfWar++;
		}

		public void ClearProvinceData(){
			ClosestProvinces.Clear();
			OverseasProvinces.Clear();
		}
	}
}