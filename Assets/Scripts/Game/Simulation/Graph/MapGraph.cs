using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation {
	public class MapGraph : MonoBehaviour, ISearchableGraph<Province, ProvinceLink> {
		private readonly Dictionary<Color32, Province> provinces = new();
		public IEnumerable<Province> Nodes => provinces.Values;
		public Province this[Color32 color]{
			get {
				provinces.TryGetValue(color, out Province value);
				return value;
			}
		}
		public void Add(Province province){
			provinces.Add(province.ColorKey, province);
		}
		public float Heuristic(Province start, Province goal){
			return Vector2.Distance(goal.MapPosition, start.MapPosition);
		}
	}
}
