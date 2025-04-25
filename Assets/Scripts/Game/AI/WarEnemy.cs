using System.Collections.Generic;
using System.Linq;
using Simulation;
using Simulation.Military;

namespace AI {
	internal class WarEnemy {
		private readonly AIController Controller;
		public readonly Country Country;
		public readonly List<Land> ClosestProvinces;
		private readonly List<Land> overseasProvinces;
		private readonly List<List<Land>> overseasLandChunks;
		private int monthsOfWar;

		public bool HasOverseasLand => overseasLandChunks.Count > 0;
		public int MonthsOfWar => monthsOfWar;
			
		public WarEnemy(AIController controller, Country country){
			Controller = controller;
			Country = country;
			ClosestProvinces = new List<Land>();
			overseasProvinces = new List<Land>();
			overseasLandChunks = new List<List<Land>>();
			monthsOfWar = 0;
		}

		public void TickMonth(){
			monthsOfWar++;
		}

		public void AddOverseasProvince(Land province){
			overseasProvinces.Add(province);
		}
		
		public void GroupOverseasProvinces(){
			while (overseasProvinces.Count > 0){
				List<Land> landChunk = new();
				Land firstProvince = overseasProvinces[^1];
				overseasProvinces.RemoveAt(overseasProvinces.Count-1);
				landChunk.Add(firstProvince);
				for (int i = overseasProvinces.Count-1; i >= 0; i--){
					Land province = overseasProvinces[i];
					if (Regiment.GetPath(firstProvince.ArmyLocation, province.ArmyLocation, LinkEvaluator) != null){
						overseasProvinces.RemoveAt(i);
					}
				}
				
				overseasLandChunks.Add(landChunk);
			}
			// TODO: Sort chunks based on distance from a chosen closest port.
		}
		private bool LinkEvaluator(ProvinceLink link){
			return Regiment.LinkEvaluator(link, false, Controller.Country);
		}

		public List<Land> GetLandChunk(Land exampleLand){
			List<Land> landChunk = overseasLandChunks.FirstOrDefault(landChunk => landChunk.Any(land => land == exampleLand));
			return landChunk ?? ClosestProvinces;
		}
		
		public void ClearProvinceData(){
			ClosestProvinces.Clear();
			overseasProvinces.Clear();
			overseasLandChunks.Clear();
		}
		
		public override string ToString(){
			return Country.ToString();
		}
	}
}