using System.Collections.Generic;
using Graphs;
using UnityEngine;

namespace Simulation {
	[RequireComponent(typeof(Calendar))]
	public class MapGraph : MonoBehaviour, ISearchableGraph<Province, ProvinceLink> {
		[SerializeField] private Transform provinceParent;
		[SerializeField] private Transform countryParent;
		[SerializeField] private Transform militaryUnitRoot;

		private readonly Dictionary<Color32, Province> provinces = new();
		private readonly List<Province> landProvinces = new();
		private readonly Dictionary<string, Country> countries = new();
		private readonly Dictionary<(Country, Country), DiplomaticStatus> diplomaticStatuses = new();
		private Bounds bounds;
		
		public Calendar Calendar {get; private set;}
		
		public Transform ProvinceParent => provinceParent;
		public Transform CountryParent => countryParent;
		public Transform MilitaryUnitRoot => militaryUnitRoot;
		public Province this[Color32 color]{get {
			provinces.TryGetValue(color, out Province value);
			return value;
		}}
		public IEnumerable<Province> Nodes => provinces.Values;
		public IEnumerable<Province> LandProvinces => landProvinces;
		public IEnumerable<Country> Countries => countries.Values;
		public Bounds Bounds => bounds;
		
		private void Awake(){
			Calendar = GetComponent<Calendar>();
		}
		
		private void Start(){
			Calendar.OnMonthTick.AddListener(() => {
				foreach (Country country in countries.Values){
					country.ClearResourceChanges();
				}
			});
		}
		
		public Country GetCountry(string countryName){
			countries.TryGetValue(countryName, out Country country);
			return country;
		}
		internal DiplomaticStatus GetDiplomaticStatus(Country a, Country b){
			if (a == b || a == null || b == null){
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
			bounds.Encapsulate(province.Bounds);
		}
		internal void Add(Country country){
			countries.Add(country.Name, country);
		}
		
		public float Heuristic(Province start, Province goal){
			return Vector2.Distance(goal.MapPosition, start.MapPosition);
		}
	}
}
