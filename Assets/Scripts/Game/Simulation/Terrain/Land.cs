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
		private Country owner;
		private float goldProduction;
		private int manpowerProduction;
		private int sailorsProduction;
		
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
		public bool HasOwner => owner != null;
		
		public void Init(Color32 colorKey, MapGraph mapGraph, ProvinceData data, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
			Province = GetComponent<Province>();
			// All land is assumed LandLocked by default, is updated to Coast if a Link to a sea tile is added.
			Province.Init(colorKey, mapGraph, Province.Type.LandLocked, data.Terrain, data.Color, mapPosition, outlineMesh, shapeMesh);
			ProvinceList.Add(Province);
			// Hides the occupation material and creates a material instance that can be modified independently in the future.
			Province.ShapeMeshRenderer.materials[1].color = Color.clear;
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
			});
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
