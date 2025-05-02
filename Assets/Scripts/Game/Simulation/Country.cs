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
		[Space]
		[SerializeField] private MeshFilter borderMeshFilter;
		[SerializeField] private MeshRenderer borderMeshRenderer;
		[SerializeField] private float borderHalfWidth;
		[SerializeField] private float borderBrightnessFactor;
		[Space]
		[SerializeField] private float monthlyManpowerDecay;
		[SerializeField] private float monthlySailorsDecay;
		#endregion
		#region Private Fields
		private readonly HashSet<Land> provinces = new();
		private readonly HashSet<Land> occupations = new();
		private readonly List<Military.Regiment> regiments = new();
		private readonly List<Military.Ship> ships = new();
		private readonly List<(float, string, Type)> monthlyGoldChanges = new();
		private readonly List<(int, string, Type)> monthlyManpowerChanges = new();
		private readonly List<(int, string, Type)> monthlySailorsChanges = new();
		private bool wasBorderChanged;
		private bool isSelected;
		
		internal readonly UnityEvent<Military.Regiment> RegimentBuilt = new();
		internal readonly UnityEvent<Military.Ship> ShipBuilt = new();
		internal readonly UnityEvent<Military.Location<Military.Regiment>> LandBattleEnded = new();
		internal readonly UnityEvent<Military.Location<Military.Ship>> SeaBattleEnded = new();
		internal readonly UnityEvent<Land> SiegeEnded = new();
		#endregion
		#region Auto-Properties
		public Color MapColor {get; private set;}
		public Land Capital {get; private set;}
		public int ProvinceCount {get; private set;}
		public int TotalDevelopment {get; private set;}
		public float Gold {get; private set;}
		public int Manpower {get; private set;}
		public int Sailors {get; private set;}
		public float GoldIncome {get; private set;}
		public int ManpowerIncome {get; private set;}
		public int SailorsIncome {get; private set;}
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
		public IReadOnlyList<(float, string, Type)> MonthlyGoldChanges => monthlyGoldChanges;
		public IReadOnlyList<(int, string, Type)> MonthlyManpowerChanges => monthlyManpowerChanges;
		public IReadOnlyList<(int, string, Type)> MonthlySailorsChanges => monthlySailorsChanges;
		public string Name => gameObject.name;
		#endregion
		
		#region Initialization
		public void Init(CountryData data, MapGraph mapGraph){
			gameObject.name = data.Name;
			MapColor = data.MapColor;
			Map = mapGraph;
			Map.Add(this);
			Capital = Map[data.Capital].Land;
			foreach (Color32 province in data.Provinces){
				Map[province].Land.Owner = this;
			}
			if (Capital.Owner != this){
				NewCapital();
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
		private void Start(){
			Map.Calendar.OnMonthTick.AddListener(AddUpMonthlyResourceChanges);
		}
		#endregion
		
		#region Changing Resource Amounts
		internal void ClearResourceChanges(){
			monthlyGoldChanges.Clear();
			monthlyManpowerChanges.Clear();
			monthlySailorsChanges.Clear();
		}
		internal void MonthlyGoldChange(float gold, string source, Type sourceType){
			monthlyGoldChanges.Add((gold, source, sourceType));
		}
		internal void MonthlyManpowerChange(int manpower, string source, Type sourceType){
			monthlyManpowerChanges.Add((manpower, source, sourceType));
		}
		internal void MonthlySailorsChange(int sailors, string source, Type sourceType){
			monthlySailorsChanges.Add((sailors, source, sourceType));
		}
		
		private void AddUpMonthlyResourceChanges(){
			GoldIncome = 0;
			ManpowerIncome = 0;
			SailorsIncome = 0;
			foreach ((float goldChange, _, _) in monthlyGoldChanges){
				GoldIncome += goldChange;
			}
			MonthlyManpowerChange((int)(Manpower*monthlyManpowerDecay), "Reserves Retiring", GetType());
			foreach ((int manpowerChange, _, _) in monthlyManpowerChanges){
				ManpowerIncome += manpowerChange;
			}
			MonthlySailorsChange((int)(Sailors*monthlySailorsDecay), "Reserves Retiring", GetType());
			foreach ((int sailorsChange, _, _) in monthlySailorsChanges){
				SailorsIncome += sailorsChange;
			}
			InstantResourceChange(GoldIncome, ManpowerIncome, SailorsIncome);
		}
		public void InstantResourceChange(float gold, int manpower, int sailors){
			Gold += gold;
			Manpower += manpower;
			Sailors += sailors;
		}
		private void OnDisable(){
			Map.Calendar.OnMonthTick.RemoveListener(AddUpMonthlyResourceChanges);
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
			if (newRegiment.IsBuilt){
				newRegiment.OnFinishBuilding();
			}
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
			if (newShip.IsBuilt){
				newShip.OnFinishBuilding();
			}
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
		
		#region Starting and Ending Wars
		public void DeclareWar(Country other){
			GetDiplomaticStatus(other).DeclareWar(other);
			// Only need to check for battles for one side, since a battle will always include both sides.
			foreach (Military.Ship ship in ships){
				ship.Location.RecheckIfBattleShouldStart();
			}
			this.RefreshSieges();
			other.RefreshSieges();
		}

		private void RefreshSieges(){
			foreach (Military.Regiment regiment in regiments){
				regiment.Location.Refresh();
			}
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
			foreach (Military.Regiment regiment in regiments.ToArray()){
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
			if (isSelected){
				province.Province.OnSelect();
			}
			if (province.Occupier == this){
				province.Unoccupy();
			}
			ChangeProvinceCount(provinces.Add(province), +1, province);
			if (ProvinceCount > 0){
				enabled = true;
			}
		}
		internal void LoseProvince(Land province){
			if (isSelected){
				province.Province.OnDeselect();
			}
			ChangeProvinceCount(provinces.Remove(province), -1, province);
			if (ProvinceCount > 0){
				if (province == Capital){
					NewCapital();
				}
				return;
			}
			Capital = null;
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
		private void ChangeProvinceCount(bool wasChanged, int change, Land province){
			if (!wasChanged){
				return;
			}
			ProvinceCount += change;
			TotalDevelopment += change*province.Development;
			wasBorderChanged = true;
		}
		internal void GainOccupation(Land province){
			occupations.Add(province);
		}
		internal void LoseOccupation(Land province){
			occupations.Remove(province);
		}
		private void NewCapital(){
			Capital = provinces.First();
			foreach (Land province in provinces){
				if (Capital.Development < province.Development){
					Capital = province;
				}
			}
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
			isSelected = true;
			foreach (Land land in Provinces){
				land.Province.OnSelect();
			}
		}
		public void OnDeselect(){
			isSelected = false;
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
		[SerializeField] private Color32 capital;
		[SerializeField] private Color32[] provinces;

		public string Name => name;
		public Color MapColor => mapColor;
		public Color32 Capital => capital;
		public IEnumerable<Color32> Provinces => provinces;
	}
}
