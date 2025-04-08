using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;

namespace Simulation {
	public class Country : MonoBehaviour, ISelectable {
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
			DiplomaticStatuses.Add((a, b), diplomaticStatus);
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
		
		private readonly HashSet<Land> provinces = new();
		private readonly HashSet<Land> occupations = new();
		private readonly List<Military.Regiment> regiments = new();
		private readonly List<Military.Ship> ships = new();
		private bool wasBorderChanged;
		
		public Color MapColor {get; private set;}
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
		public IEnumerable<Land> Provinces => provinces;
		public IEnumerable<Land> Occupations => occupations;
		public IEnumerable<Land> ControlledLand {
			get {
				foreach (Land province in provinces){
					if (!province.IsOccupied){
						yield return province;
					}
				}
				foreach (Land province in occupations){
					yield return province;
				}
			}
		}
		// TODO: Assign a specific province as capital from country data.
		public Land Capital => provinces.First();
		public int RegimentCount => regiments.Count;
		public int ShipCount => ships.Count;
		public string Name => gameObject.name;
		
		public void Init(CountryData data, MapGraph map){
			gameObject.name = data.Name;
			MapColor = data.MapColor;
			foreach (Color32 province in data.Provinces){
				map[province].Land.Owner = this;
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
			HashSet<List<Province>> borderProvinceLoops = new();
			HashSet<Province> unsearchedProvinces = new(provinces.Select(land => land.Province));
			while (unsearchedProvinces.Count > 0){
				Province borderProvince = unsearchedProvinces.First();
				unsearchedProvinces.Remove(borderProvince);
				for (int segmentIndex = 0; segmentIndex < borderProvince.OutlineSegments.Count; segmentIndex++){
					(int _, int _, ProvinceLink link) = borderProvince.OutlineSegments[segmentIndex];
					if (link != null && link.Target.IsLand && link.Target.Land.Owner == this){
						continue;
					}
					List<Province> provinceLoop = new(){borderProvince};
					ProvinceLink firstLink = null;
					while (true){
						segmentIndex = (segmentIndex+1)%borderProvince.OutlineSegments.Count;
						(_, _, link) = borderProvince.OutlineSegments[segmentIndex];
						if (link == firstLink){
							if (provinceLoop.Count > 1){
								// Remove the last province because it's a duplicate of loopStartProvince.
								provinceLoop.RemoveAt(provinceLoop.Count-1);
							}
							break;
						}
						firstLink ??= link;
						if (link == null || link.Target.IsSea || link.Target.Land.Owner != this){
							continue;
						}
						segmentIndex = link.Target[borderProvince.ColorKey].SegmentIndex;
						borderProvince = link.Target;
						unsearchedProvinces.Remove(borderProvince);
						provinceLoop.Add(borderProvince);
					}
					borderProvinceLoops.Add(provinceLoop);
					break;
				}
			}
			foreach (List<Province> provinceLoop in borderProvinceLoops){
				List<Vector2> vertexLoop;
				if (provinceLoop.Count == 1){
					vertexLoop = new List<Vector2>(provinceLoop[0].Vertices);
					for (int i = 0; i < vertexLoop.Count; i++){
						vertexLoop[i] += provinceLoop[0].MapPosition;
					}
					PolygonOutline.GenerateMeshData(borderMeshData, vertexLoop, borderHalfWidth, true);
					continue;
				}
				vertexLoop = new List<Vector2>();
				Province previousProvince = provinceLoop[^1];
				Province currentProvince = provinceLoop[0];
				for (int provinceIndex = 0; provinceIndex < provinceLoop.Count; provinceIndex++){
					Province nextProvince = provinceLoop[(provinceIndex+1)%provinceLoop.Count];
					int segmentIndex = (currentProvince[previousProvince.ColorKey].SegmentIndex+1)%currentProvince.OutlineSegments.Count;
					while (true){
						(int vertexIndex, int endIndex, ProvinceLink link) = currentProvince.OutlineSegments[segmentIndex];
						if (link.Target == nextProvince){
							break;
						}
						for (; vertexIndex != endIndex; vertexIndex = (vertexIndex+1)%currentProvince.Vertices.Count){
							vertexLoop.Add(currentProvince.MapPosition+currentProvince.Vertices[vertexIndex]);
						}
						segmentIndex = (segmentIndex+1)%currentProvince.OutlineSegments.Count;
					}
					previousProvince = currentProvince;
					currentProvince = nextProvince;
				}
				PolygonOutline.GenerateMeshData(borderMeshData, vertexLoop, borderHalfWidth, true);
			}
			borderMeshFilter.mesh = borderMeshData.ToMesh();
			wasBorderChanged = false;
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
		}
		
		internal void GainProvince(Land province){
			if (province.Occupier == this){
				province.Unoccupy();
			}
			ChangeProvinceCount(provinces.Add(province), +1);
		}
		internal void LoseProvince(Land province){
			ChangeProvinceCount(provinces.Remove(province), -1);
		}
		private void ChangeProvinceCount(bool wasChanged, int change){
			if (!wasChanged){
				return;
			}
			ProvinceCount += change;
			wasBorderChanged = true;
		}
		internal void GainOccupation(Land province){
			occupations.Add(province);
		}
		internal void LoseOccupation(Land province){
			occupations.Remove(province);
		}

		public bool TryStartRecruitingRegiment(Military.RegimentType type, Province province){
			if (!regimentTypes.Contains(type) || province.IsSea || province.Land.Controller != this){
				return false;
			}
			Military.Regiment newRegiment = Military.Regiment.StartCreating(type, province.Land.ArmyLocation, this);
			if (newRegiment == null){
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
			if (province.Land.Owner != this && !GetDiplomaticStatus(province.Land.Owner).IsAtWar){
				return Military.MoveOrderResult.NoAccess;
			}
			return regiment.MoveTo(province.Land.ArmyLocation);
		}
		public bool TryStartConstructingFleet(Military.ShipType type, Military.Harbor location){
			if (!shipTypes.Contains(type) || location == null || location.Land.Controller != this){
				return false;
			}
			Military.Ship newShip = Military.Ship.StartCreating(type, location, this);
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
			if (location.Province.IsLand && location.Province.Land.Owner != this && !GetDiplomaticStatus(location.Province.Land.Owner).IsAtWar){
				return Military.MoveOrderResult.NoAccess;
			}
			return ship.MoveTo(location);
		}

		public void EndWar(Country opponent, PeaceTreaty treaty){
			DiplomaticStatus diplomaticStatus = GetDiplomaticStatus(opponent);
			if (!diplomaticStatus.IsAtWar){
				return;
			}
			this.Unoccupy(opponent);
			opponent.Unoccupy(this);
			treaty.Apply();
			diplomaticStatus.EndWar(treaty.TruceLength);
			this.RetreatFrom(opponent);
			opponent.RetreatFrom(this);
		}
		private void Unoccupy(Country other){
			foreach (Land occupiedLand in occupations.ToArray()){
				if (occupiedLand.Owner == other){
					occupiedLand.Unoccupy();
				}
			}
		}
		private void RetreatFrom(Country other){
			foreach (Military.Regiment regiment in regiments){
				if (regiment.Location.Province.Land.Owner == other){
					regiment.RetreatHome();
				}
			}
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
			foreach (Land land in Provinces){
				land.Province.OnSelect();
			}
		}
		public void OnDeselect(){
			foreach (Land land in Provinces){
				land.Province.OnDeselect();
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
