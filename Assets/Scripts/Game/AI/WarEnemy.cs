using System.Collections.Generic;
using System.Linq;
using Simulation;
using Simulation.Military;

namespace AI {
	internal class WarEnemy {
		private readonly AIController Controller;
		public readonly Country Country;
		public readonly List<Land> ClosestProvinces;
		private readonly List<Land> overseasLandLocked;
		private readonly List<Land> overseasCoast;
		private readonly HashSet<List<Land>> overseasLandChunks;
		private int monthsOfWar;

		public bool HasOverseasLand => overseasLandChunks.Count > 0;
		public int MonthsOfWar => monthsOfWar;
			
		public WarEnemy(AIController controller, Country country){
			Controller = controller;
			Country = country;
			ClosestProvinces = new List<Land>();
			overseasLandLocked = new List<Land>();
			overseasCoast = new List<Land>();
			overseasLandChunks = new HashSet<List<Land>>();
			monthsOfWar = 0;
		}

		public void TickMonth(){
			monthsOfWar++;
		}

		public void AddOverseasProvince(Land province){
			(province.Province.IsCoast ? overseasCoast : overseasLandLocked).Add(province);
		}
		
		public void GroupOverseasProvinces(){
			AddLandChunks(overseasCoast);
			AddLandChunks(overseasLandLocked);
		}
		private void AddLandChunks(List<Land> overseasLands){
			while (overseasLands.Count > 0){
				List<Land> landChunk = new();
				Dictionary<Land, int> distances = new();
				Land firstProvince = overseasLands[^1];
				overseasLands.RemoveAt(overseasLands.Count-1);
				landChunk.Add(firstProvince);
				distances.Add(firstProvince, 0);
				AddConnectedLand(firstProvince, overseasCoast, landChunk, distances);
				AddConnectedLand(firstProvince, overseasLandLocked, landChunk, distances);
				landChunk.Sort((left, right) => distances[left]-distances[right]);
				overseasLandChunks.Add(landChunk);
			}
		}
		private void AddConnectedLand(Land firstProvince, List<Land> overseasLands, List<Land> landChunk, Dictionary<Land, int> distances){
			const float speedIsIrrelevantForSorting = 1;
			for (int i = overseasLands.Count-1; i >= 0; i--){
				Land province = overseasLands[i];
				List<ProvinceLink> path = Regiment.GetPath(firstProvince.ArmyLocation, province.ArmyLocation, LinkEvaluator);
				if (path == null){
					continue;
				}
				overseasLands.RemoveAt(i);
				landChunk.Add(province);
				distances[province] = path.Sum(link => Regiment.GetTravelDays(link, speedIsIrrelevantForSorting));
			}
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
			overseasLandLocked.Clear();
			overseasCoast.Clear();
			overseasLandChunks.Clear();
		}
		
		public override string ToString(){
			return Country.ToString();
		}
	}
}