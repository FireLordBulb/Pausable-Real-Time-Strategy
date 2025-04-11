using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation {
	[RequireComponent(typeof(Calendar))]
	public class MapGraph : MonoBehaviour, ISearchableGraph<Province, ProvinceLink> {
		[SerializeField] private Transform militaryUnitRoot;

		private readonly Dictionary<Color32, Province> provinces = new();
		private readonly List<Province> landProvinces = new();
		private readonly Dictionary<string, Country> countries = new();
		private readonly Dictionary<(Country, Country), DiplomaticStatus> diplomaticStatuses = new();

		public Calendar Calendar {get; private set;}
		
		public Transform MilitaryUnitRoot => militaryUnitRoot;
		public Province this[Color32 color]{
			get {
				provinces.TryGetValue(color, out Province value);
				return value;
			}
		}
		public IEnumerable<Province> Nodes => provinces.Values;
		public IEnumerable<Province> LandProvinces => landProvinces;
		
		private void Awake(){
			Calendar = GetComponent<Calendar>();
		}
		
		public Country GetCountry(string countryName){
			countries.TryGetValue(countryName, out Country country);
			return country;
		}
		internal DiplomaticStatus GetDiplomaticStatus(Country a, Country b){
			if (a == b){
				return null;
			}
			if (diplomaticStatuses.TryGetValue((a, b), out DiplomaticStatus diplomaticStatus)){
				return diplomaticStatus;
			}
			if (diplomaticStatuses.TryGetValue((b, a), out diplomaticStatus)){
				return diplomaticStatus;
			}
			diplomaticStatus = new DiplomaticStatus(Calendar);
			diplomaticStatuses.Add((a, b), diplomaticStatus);
			return diplomaticStatus;
		}
		
		internal void Add(Province province){
			provinces.Add(province.ColorKey, province);
			if (province.IsLand){
				landProvinces.Add(province);
			}
		}
		internal void Add(Country country){
			countries.Add(country.Name, country);
		}
		
		public float Heuristic(Province start, Province goal){
			return Vector2.Distance(goal.MapPosition, start.MapPosition);
		}
	}
}
