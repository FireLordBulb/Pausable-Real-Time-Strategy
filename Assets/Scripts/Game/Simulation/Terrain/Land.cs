using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation {
	[RequireComponent(typeof(Province))]
	public class Land : MonoBehaviour {
		private static readonly List<Province> ProvinceList = new();
		public static IEnumerable<Province> AllProvinces => ProvinceList;
#if UNITY_EDITOR
		public static void ClearProvinceList(){
			ProvinceList.Clear();
		}
#endif
		[SerializeField] private int occupationMaterialIndex;
		[SerializeField] private float siegeDaysPerDevelopment;
		
		private Country owner;
		private Country occupier;
		
		private float goldProduction;
		private int manpowerProduction;
		private int sailorsProduction;

		private Material occupationMaterial;
		
		public Province Province {get; private set;}
		public Terrain Terrain {get; private set;}
		public Military.LandLocation ArmyLocation {get; private set;}
		
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
			}
		}
		public Country Occupier => occupier;
		public Country Controller => Occupier == null ? Owner : Occupier;
		public bool HasOwner => owner != null;
		public bool IsOccupied => occupier != null;

		public int SiegeDays => (int)(siegeDaysPerDevelopment*(1+Terrain.DevelopmentModifier));
		
		public void Init(Color32 colorKey, MapGraph mapGraph, ProvinceData data, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
			Province = GetComponent<Province>();
			// All land is assumed LandLocked by default, is updated to Coast if a Link to a sea tile is added.
			Province.Init(colorKey, mapGraph, Province.Type.LandLocked, data.Terrain, data.Color, mapPosition, outlineMesh, shapeMesh);
			ProvinceList.Add(Province);

			occupationMaterial = Province.MeshRenderer.sharedMaterials[occupationMaterialIndex];
			occupationMaterial.color = Color.clear;
			Terrain = Province.Terrain;
			ArmyLocation = new Military.LandLocation(this);
		}
		
		// TODO: Refactor away and put values in an SO.
		private const int BaseProduction = 10;
		private void Start(){
			float multiplier = 1+Terrain.DevelopmentModifier;
			goldProduction = multiplier;
			manpowerProduction = Mathf.RoundToInt(BaseProduction*multiplier);
			sailorsProduction = Province.IsCoast ? Mathf.RoundToInt(BaseProduction*multiplier) : 0;
			Calendar.Instance.OnMonthTick.AddListener(() => {
				if (!HasOwner){
					return;
				}               
				Owner.GainResources(goldProduction, manpowerProduction, sailorsProduction);
			}, GetType());
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
		[SerializeField] private Color32 color;
		[SerializeField] private Terrain terrain;
		public Color32 Color => color;
		public Terrain Terrain => terrain;
	}
}
