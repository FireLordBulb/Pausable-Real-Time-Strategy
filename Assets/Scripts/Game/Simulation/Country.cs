using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;

namespace Simulation {
	public class Country : MonoBehaviour {
		private static Transform militaryUnitParent;
		private static readonly Dictionary<string, Country> Countries = new();
		private static readonly Dictionary<(Country, Country), DiplomaticStatus> DiplomaticStatuses = new();
		public static Country Get(string key){
			Countries.TryGetValue(key, out Country country);
			return country;
		}
		private static DiplomaticStatus GetDiplomaticStatus(Country a, Country b){
			if (a == b){
				return null;
			}
			if (DiplomaticStatuses.TryGetValue((a, b), out DiplomaticStatus diplomaticStatus)){
				return diplomaticStatus;
			}
			if (DiplomaticStatuses.TryGetValue((b, a), out diplomaticStatus)){
				return diplomaticStatus;
			}
			diplomaticStatus = new DiplomaticStatus();
			DiplomaticStatuses.Add((a, b), new DiplomaticStatus());
			return diplomaticStatus;
		}
#if UNITY_EDITOR
		public static void ClearCountryDictionary(){
			Countries.Clear();
			DiplomaticStatuses.Clear();
		}
#endif
		
		[SerializeField] private Military.RegimentType[] regimentTypes;
		[SerializeField] private Military.ShipType[] shipTypes;
		[SerializeField] private MeshFilter borderMeshFilter;
		[SerializeField] private MeshRenderer borderMeshRenderer;
		[SerializeField] private float borderHalfWidth;
		[SerializeField] private float borderBrightnessFactor;
		
		private readonly HashSet<Province> provinces = new();
		private readonly List<Military.Regiment> regiments = new();
		private readonly List<Military.Ship> ships = new();
		private bool wasBorderChanged;
		
		public Color MapColor {get; private set;}
		public bool IsDirty {get; private set;}
		public int ProvinceCount {get; private set;}
		public float Gold {get; private set;}
		public int Manpower {get; private set;}
		public int Sailors {get; private set;}
		
		public Transform MilitaryUnitParent {
			get {
				if (militaryUnitParent == null){
					militaryUnitParent = new GameObject("MilitaryUnits"){
						transform = {
							// TODO: Replace with passed down reference when singletons are removed.
							parent = transform.parent.parent
						}
					}.transform;
				}
				return militaryUnitParent;
			}
		}
		public IEnumerable<Military.RegimentType> RegimentTypes => regimentTypes;
		public IEnumerable<Military.ShipType> ShipTypes => shipTypes;
		public IEnumerable<Province> Provinces => provinces;
		// TODO: Assign a specific province as capital from country data.
		public Province Capital => provinces.First();
		public int RegimentCount => regiments.Count;
		public int ShipCount => ships.Count;
		public string Name => gameObject.name;
		
		public void Init(CountryData data, MapGraph map){
			gameObject.name = data.Name;
			MapColor = data.MapColor;
			foreach (Color32 province in data.Provinces){
				map[province].Owner = this;
			}
			Color borderColor = MapColor*borderBrightnessFactor;
			borderColor.a = 1;
			borderMeshRenderer.material.color = borderColor;
			RegenerateBorder();
			Countries.Add(Name, this);
		}
		private void RegenerateBorder(){
			DestroyImmediate(borderMeshFilter.sharedMesh);
			if (provinces.Count == 0){
				return;
			}
			MeshData borderMeshData = new($"{gameObject.name}BorderMesh");
			List<Vector2> borderVertices = new();
			
			// TODO: Only add the sections of vertices between outer border tri-points.
			Province province = provinces.First();
			int startSegment = 0;
			ProvinceLink link = null;
			AddAllButOneSegments();
			startSegment = (link.Target[province.ColorKey].SegmentIndex+1)%link.Target.outlineSegments.Count;
			province = link.Target;
			AddAllButOneSegments();
			
			// Completes incomplete loops
			/*for (int i = borderVertices.Count-2; i > 0; i--){
				borderVertices.Add(borderVertices[i]+Vector2.up*5);
			}*/
			
			PolygonOutline.GenerateMeshData(borderMeshData, borderVertices, borderHalfWidth, true);
			borderMeshFilter.mesh = borderMeshData.ToMesh();
			wasBorderChanged = false;

			void AddAllButOneSegments(){
				for (int i = 0; i < province.outlineSegments.Count; i++){
					int index = (i+startSegment+province.outlineSegments.Count)%province.outlineSegments.Count;
					int startIndex, endIndex;
					(startIndex, endIndex, link) = province.outlineSegments[index];
					if (i >= province.outlineSegments.Count-1){
						break;
					}
					for (int j = startIndex; j != endIndex; j = (j+1)%province.Vertices.Count){
						borderVertices.Add(province.MapPosition+province.Vertices[j]);
					}
				}
			}
		}
		
		private void Update(){
			if (wasBorderChanged){
				RegenerateBorder();
			}
		}
		
		public void GainResources(float gold, int manpower, int sailors){
			Gold += gold;
			Manpower += manpower;
			Sailors += sailors;
			IsDirty = true;
		}
		public void MarkClean(){
			IsDirty = false;
		}
		
		internal bool GainProvince(Province province){
			return ChangeProvinceCount(provinces.Add(province), +1);
		}
		internal bool LoseProvince(Province province){
			return ChangeProvinceCount(provinces.Remove(province), -1);
		}
		private bool ChangeProvinceCount(bool wasChanged, int change){
			if (!wasChanged){
				return false;
			}
			ProvinceCount += change;
			wasBorderChanged = true;
			IsDirty = true;
			return true;
		}

		public bool TryStartRecuitingRegiment(Military.RegimentType type, Province province){
			if (!regimentTypes.Contains(type) || province.Owner != this){
				return false;
			}
			Military.Unit<Military.Regiment> newArmyUnit = Military.Regiment.StartCreating(type, province.Land.ArmyLocation, this);
			if (newArmyUnit == null){
				return false;
			}
			if (newArmyUnit is not Military.Regiment newRegiment){
				Debug.LogError($"Army unit of unknown type '{newArmyUnit.Type.name}' in {this}'s regimentTypes list!");
				Destroy(newArmyUnit);
				return false;
			}
			regiments.Add(newRegiment);
			return true;
		}
		public Military.MoveOrderResult MoveRegimentTo(Military.Regiment regiment, Province province){
			if (regiment.Owner != this){
				return Military.MoveOrderResult.NotOwner;
			}
			if (province.IsSea){
				return Military.MoveOrderResult.InvalidTarget;
			}
			if (province.Owner != this && !GetDiplomaticStatus(province.Owner).IsAtWar){
				return Military.MoveOrderResult.NoAccess;
			}
			return regiment.MoveTo(province.Land.ArmyLocation);
		}
		public bool TryStartConstructingFleet(Military.ShipType type, Military.Harbor location){
			if (!shipTypes.Contains(type) || location == null || location.Land.Province.Owner != this){
				return false;
			}
			Military.Unit<Military.Ship> newNavyUnit = Military.Ship.StartCreating(type, location, this);
			if (newNavyUnit == null){
				return false;
			}
			if (newNavyUnit is not Military.Ship newShip){
				Debug.LogError($"Navy unit of unknown type '{newNavyUnit.Type.name}' in {this}'s shipTypes list!");
				Destroy(newNavyUnit);
				return false;
			}
			if (newShip == null){
				return false;
			}
			ships.Add(newShip);
			return true;
		}
		public Military.MoveOrderResult MoveFleetTo(Military.Ship ship, Military.Location<Military.Ship> location){
			if (ship.Owner != this){
				return Military.MoveOrderResult.NotOwner;
			}
			Country owner = location.Province.Owner;
			if (location.Province.IsLand && owner != this && !GetDiplomaticStatus(owner).IsAtWar){
				return Military.MoveOrderResult.NoAccess;
			}
			return ship.MoveTo(location);
		}

		internal void RemoveRegiment(Military.Regiment regiment){
			regiments.Remove(regiment);
		}
		internal void RemoveShip(Military.Ship ship){
			ships.Remove(ship);
		}
		
		public DiplomaticStatus GetDiplomaticStatus(Country other){
			return GetDiplomaticStatus(this, other);
		}
		
		public void OnSelect(){
			foreach (Province province in Provinces){
				province.OnSelect();
			}
		}
		public void OnDeselect(){
			foreach (Province province in Provinces){
				province.OnDeselect();
			}
		}
	}

	[Serializable]
	public class CountryData {
		[SerializeField] private string name;
		[SerializeField] private Color mapColor;
		[SerializeField] private Color32[] provinces;

		public string Name => name;
		public Color MapColor => mapColor;
		public IEnumerable<Color32> Provinces => provinces;
	}
}
