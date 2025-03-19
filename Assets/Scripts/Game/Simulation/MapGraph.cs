	using System.Collections.Generic;
	using Graphs;
	using UnityEngine;

	public class MapGraph : MonoBehaviour, ISearchableGraph<Province, ProvinceLink> {
		private readonly Dictionary<Color, Province> provinces = new();
		public IEnumerable<Province> Nodes => provinces.Values;
		public void Add(Color color, Province province){
			provinces.Add(color, province);
		}
		public float Heuristic(Province start, Province goal){
			return Vector2.Distance(goal.MapPosition, start.MapPosition);
		}
	}
