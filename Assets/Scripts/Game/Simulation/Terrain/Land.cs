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
		
		public Province Province {get; private set;}
		public Terrain Terrain {get; private set;}
		public Military.LandLocation ArmyLocation {get; private set;}
		
		public void Init(Color32 colorKey, MapGraph mapGraph, ProvinceData data, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
			Province = GetComponent<Province>();
			// All land is assumed LandLocked by default, is updated to Coast if a Link to a sea tile is added.
			Province.Init(colorKey, mapGraph, Province.Type.LandLocked, data.Terrain, data.Color, mapPosition, outlineMesh, shapeMesh);
			ProvinceList.Add(Province);
			Terrain = Province.Terrain;
			ArmyLocation = new Military.LandLocation(this);
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
