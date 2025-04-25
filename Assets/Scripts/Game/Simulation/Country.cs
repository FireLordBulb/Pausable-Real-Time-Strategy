using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;
using UnityEngine.Events;

namespace Simulation {
	public class Country : MonoBehaviour, ISelectable {
		#region Serialized Fields
		[SerializeField] private Military.RegimentType[] regimentTypes;
		[SerializeField] private Military.ShipType[] shipTypes;
		[SerializeField] private TruceData truceData;
		[SerializeField] private MeshFilter borderMeshFilter;
		[SerializeField] private MeshRenderer borderMeshRenderer;
		[SerializeField] private float borderHalfWidth;
		[SerializeField] private float borderBrightnessFactor;
		#endregion
		#region Private Fields
		private readonly HashSet<Land> provinces = new();
		private readonly HashSet<Land> occupations = new();
		private readonly List<Military.Regiment> regiments = new();
		private readonly List<Military.Ship> ships = new();
		private bool wasBorderChanged;
		
		internal readonly UnityEvent<Military.Regiment> RegimentBuilt = new();
		internal readonly UnityEvent<Military.Ship> ShipBuilt = new();
		internal readonly UnityEvent<Military.Location<Military.Regiment>> LandBattleEnded = new();
		internal readonly UnityEvent<Military.Location<Military.Ship>> SeaBattleEnded = new();
		internal readonly UnityEvent<Land> SiegeEnded = new();
		#endregion
		#region Auto-Properties
		public Color MapColor {get; private set;}
		public int ProvinceCount {get; private set;}
		public float Gold {get; private set;}
		public int Manpower {get; private set;}
		public int Sailors {get; private set;}
		public MapGraph Map {get; private set;}
		public Transform MilitaryUnitParent {get; private set;}
		#endregion
		#region Getter Properties
		public IReadOnlyList<Military.RegimentType> RegimentTypes => regimentTypes;
		public IReadOnlyList<Military.ShipType> ShipTypes => shipTypes;
		public IReadOnlyList<Military.Regiment> Regiments => regiments;
		public IReadOnlyList<Military.Ship> Ships => ships;
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
		public string Name => gameObject.name;
		#endregion
		
		#region Initialization
		public void Init(CountryData data, MapGraph mapGraph){
			gameObject.name = data.Name;
			MapColor = data.MapColor;
			Map = mapGraph;
			Map.Add(this);
			foreach (Color32 province in data.Provinces){
				Map[province].Land.Owner = this;
			}
			MilitaryUnitParent = new GameObject(gameObject.name){
				transform = {
					parent = Map.MilitaryUnitRoot
				}
			}.transform;
			
			Color borderColor = MapColor*borderBrightnessFactor;
			borderColor.a = 1;
			borderMeshRenderer.material.color = borderColor;
			RegenerateBorder();
		}
		#endregion
		
		#region Changing Resource Amounts
		public void ChangeResources(float gold, int manpower, int sailors){
			Gold += gold;
			Manpower += manpower;
			Sailors += sailors;
		}
		#endregion
		
		#region Public Interface for Military Units
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
		public Military.MoveOrderResult MoveRegimentTo(Military.Regiment regiment, Military.Location<Military.Regiment> location){
			if (regiment.Owner != this){
				return Military.MoveOrderResult.NotOwner;
			}
			if (location == null){
				return Military.MoveOrderResult.InvalidTarget;
			}
			if (location.Province.IsLand && location.Province.Land.Owner != this && !GetDiplomaticStatus(location.Province.Land.Owner).IsAtWar){
				return Military.MoveOrderResult.NoAccess;
			}
			if (location is Military.TransportDeck deck && deck.Transport.Owner != this){
				return Military.MoveOrderResult.DestinationUnusable;
			}
			return regiment.MoveTo(location);
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
			if (location == null || location.Province.IsLand && location.Province.Land.Owner != this && !GetDiplomaticStatus(location.Province.Land.Owner).IsAtWar){
				return Military.MoveOrderResult.NoAccess;
			}
			return ship.MoveTo(location);
		}
		#endregion
		
		#region Ending Wars
		public void EndWar(Country opponent, PeaceTreaty treaty){
			DiplomaticStatus diplomaticStatus = GetDiplomaticStatus(opponent);
			if (!diplomaticStatus.IsAtWar){
				return;
			}
			this.Unoccupy(opponent);
			opponent.Unoccupy(this);
			treaty.Apply();
			diplomaticStatus.EndWar(treaty.TruceLength);
			this.RetreatHome();
			opponent.RetreatHome();
		}
		private void Unoccupy(Country other){
			foreach (Land occupiedLand in occupations.ToArray()){
				if (occupiedLand.Owner == other){
					occupiedLand.Unoccupy();
				}
			}
		}
		private void RetreatHome(){
			foreach (Military.Regiment regiment in regiments){
				if (regiment.Location is Military.TransportDeck){
					continue;
				}
				Country owner = regiment.Location.Province.Land.Owner;
				if (owner != this && !GetDiplomaticStatus(owner).IsAtWar){
					regiment.RetreatHome();
				}
			}
			foreach (Military.Ship ship in ships.ToArray()){
				if (ship.Location is not Military.Harbor harbor){
					continue;
				}
				Country owner = harbor.Land.Owner;
				if (owner != this && !GetDiplomaticStatus(owner).IsAtWar){
					if (ship.IsBuilt){
						ship.MoveTo(harbor.Sea.NavyLocation);
					} else {
						ship.StackWipe();
					}
				}
			}
		}
		#endregion
		
		#region Losing Military Units
		internal void RemoveRegiment(Military.Regiment regiment){
			regiments.Remove(regiment);
		}
		internal void RemoveShip(Military.Ship ship){
			ships.Remove(ship);
		}
		#endregion
		
		#region Annexing and Occupying Land
		internal void GainProvince(Land province){
			if (province.Occupier == this){
				province.Unoccupy();
			}
			ChangeProvinceCount(provinces.Add(province), +1);
			if (ProvinceCount > 0){
				enabled = true;
			}
		}
		internal void LoseProvince(Land province){
			ChangeProvinceCount(provinces.Remove(province), -1);
			if (ProvinceCount > 0){
				return;
			}
			foreach (Military.Regiment regiment in regiments.ToArray()){
				regiment.StackWipe();
			}
			foreach (Military.Ship ship in ships.ToArray()){
				ship.StackWipe();
			}
			foreach (Land land in occupations.ToArray()){
				land.Unoccupy();
			}
			RegenerateBorder();
			enabled = false;
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
		#endregion
		
		#region Regenerating the Border Mesh
		private void Update(){
			if (wasBorderChanged){
				RegenerateBorder();
			}
		}
		private void RegenerateBorder(){
			DestroyImmediate(borderMeshFilter.sharedMesh);
			if (provinces.Count == 0){
				return;
			}
			MeshData borderMeshData = new($"{gameObject.name}BorderMesh");
			HashSet<Province> unsearchedProvinces = new(provinces.Select(land => land.Province));
			CountryBorder.Generate(borderMeshData, this, unsearchedProvinces, borderHalfWidth);
			borderMeshFilter.sharedMesh = borderMeshData.ToMesh();
			wasBorderChanged = false;
		}
		
		#endregion

		#region Public Interface for Internal UnityEvents
		public void RegimentBuiltAddListener(UnityAction<Military.Regiment> action){
			RegimentBuilt.AddListener(action);
		}
		public void RegimentBuiltRemoveListener(UnityAction<Military.Regiment> action){
			RegimentBuilt.RemoveListener(action);
		}
		public void ShipBuiltAddListener(UnityAction<Military.Ship> action){
			ShipBuilt.AddListener(action);
		}
		public void ShipBuiltRemoveListener(UnityAction<Military.Ship> action){
			ShipBuilt.RemoveListener(action);
		}
		public void LandBattleEndedAddListener(UnityAction<Military.Location<Military.Regiment>> action){
			LandBattleEnded.AddListener(action);
		}
		public void LandBattleEndedRemoveListener(UnityAction<Military.Location<Military.Regiment>> action){
			LandBattleEnded.RemoveListener(action);
		}
		public void SeaBattleEndedAddListener(UnityAction<Military.Location<Military.Ship>> action){
			SeaBattleEnded.AddListener(action);
		}
		public void SeaBattleEndedRemoveListener(UnityAction<Military.Location<Military.Ship>> action){
			SeaBattleEnded.RemoveListener(action);
		}
		public void SiegeEndedAddListener(UnityAction<Land> action){
			SiegeEnded.AddListener(action);
		}
		public void SiegeEndedRemoveListener(UnityAction<Land> action){
			SiegeEnded.RemoveListener(action);
		}
		#endregion
		
		#region Selection Interface for the UI
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
		#endregion
		
		#region Getter Methods
		public DiplomaticStatus GetDiplomaticStatus(Country other){
			return Map.GetDiplomaticStatus(this, other);
		}
		public PeaceTreaty NewPeaceTreaty(Country recipient){
			return new PeaceTreaty(this, recipient, truceData);
		}
		public override string ToString(){
			return Name;
		}
		#endregion
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
