using System;
using UnityEngine;

[RequireComponent(typeof(Province))]
public class Land : MonoBehaviour {
	public Province Province {get; private set;}
	
	public void Init(Color32 colorKey, ProvinceData data, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
		Province = GetComponent<Province>();
		// All land is assumed LandLocked by default, is updated to Coast if a Link to a sea tile is added.
		Province.Init(colorKey, Province.Type.LandLocked, data.Terrain, data.Color, mapPosition, outlineMesh, shapeMesh);
	}
}

[Serializable]
public class ProvinceData {
	[SerializeField] private Color32 color;
	[SerializeField] private Terrain terrain;
	public Color32 Color => color;
	public Terrain Terrain => terrain;
}
