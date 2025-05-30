using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation {
	[RequireComponent(typeof(Province))]
	public class Land : MonoBehaviour {
		[SerializeField] private int occupationMaterialIndex;
		[SerializeField] private BaseProduction baseProduction;
		[SerializeField] private float siegeDaysPerDevelopment;
		[SerializeField] private int ruralThreshold;
		[SerializeField] private int urbanThreshold;
		
		private Country owner;
		private Country occupier;
		
		public float GoldProduction {get; private set;}
		public int ManpowerProduction {get; private set;}
		public int SailorsProduction {get; private set;}

		private Material occupationMaterial;
		
		public Province Province {get; private set;}
		public Military.LandLocation ArmyLocation {get; private set;}
		public int Development {get; private set;}
		
		public Country Owner {
			get => owner;
			set {
				if (owner == value){
					return;
				}
				if (owner != null){
					owner.LoseProvince(this);
				}
				owner = value;
				if (owner != null){
					owner.GainProvince(this);
					Province.BaseColor = owner.MapColor;
				} else {
					Province.BaseColor = Color.black;
				}
				ArmyLocation.Refresh();
			}
		}
		public Country Occupier => occupier;
		public Country Controller => occupier == null ? owner : occupier;
		public bool HasOwner => owner != null;
		public bool IsOccupied => occupier != null;
		
		public int SiegeDays => (int)(siegeDaysPerDevelopment*Development);
		
		public void Init(Color32 colorKey, MapGraph mapGraph, ProvinceData data, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh, IEnumerable<Vector2> vertices){
			Province = GetComponent<Province>();
			string provinceName = data.Name.Trim();
			if (provinceName.Length == 0){
				provinceName = data.Development >= urbanThreshold ? $"Urban {data.Terrain.Name}" : data.Development >= ruralThreshold ? data.Terrain.Name : $"Rural {data.Terrain.Name}";
			}
			// All land is assumed LandLocked by default, is updated to Coast if a Link to a sea tile is added.
			Province.Init(provinceName, colorKey, mapGraph, data.Terrain, data.Color, mapPosition, outlineMesh, shapeMesh, vertices);

			occupationMaterial = Province.MeshRenderer.sharedMaterials[occupationMaterialIndex];
			occupationMaterial.color = Color.clear;
			// In the data development values have a minimum of 0, but are shifted up by one for all the actual computations.
			Development = 1+data.Development;
			ArmyLocation = new Military.LandLocation(this);
		}
		
		private void Start(){
			GoldProduction = baseProduction.Gold*Development*Province.GoldMultiplier;
			ManpowerProduction = Mathf.RoundToInt(baseProduction.Manpower*Development*Province.ManpowerMultiplier);
			SailorsProduction = Mathf.RoundToInt(baseProduction.Sailors*Development*Province.SailorsMultiplier);
			Province.Calendar.OnMonthTick.AddListener(() => {
				if (!HasOwner){
					return;
				}               
				Owner.MonthlyGoldChange(GoldProduction, Province.Name, GetType());
				Owner.MonthlyManpowerChange(ManpowerProduction, Province.Name, GetType());
				Owner.MonthlySailorsChange(SailorsProduction, Province.Name, GetType());
			});
		}

		internal void MakeOccupiedBy(Country country){
			if (country == Owner){
				Unoccupy();
				return;
			}
			if (occupier != null){
				occupier.LoseOccupation(this);
			}
			occupier = country;
			occupier.GainOccupation(this);
			occupationMaterial.color = occupier.MapColor;
		}
		internal void Unoccupy(){
			if (occupier != null){
				occupier.LoseOccupation(this);
			}
			occupier = null;
			occupationMaterial.color = Color.clear;
		}
	}

	[Serializable]
	public class ProvinceData {
		[SerializeField] private string name;
		[SerializeField] private Color32 color;
		[SerializeField] private Terrain terrain;
		[SerializeField] private int development;
		public string Name => name;
		public Color32 Color => color;
		public Terrain Terrain => terrain;
		public int Development => development;
	}
}
