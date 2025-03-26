using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralMeshes;
using UnityEngine;

public class Country : MonoBehaviour {
	private readonly HashSet<Province> provinces = new();
	
	[SerializeField] private MeshFilter borderMeshFilter;
	[SerializeField] private MeshRenderer borderMeshRenderer;
	[SerializeField] private float borderHalfWidth;
	[SerializeField] private float borderBrightnessFactor;
	
	private bool wasBorderChanged;
	
	public Color MapColor {get; private set;}
	public IEnumerable<Province> Provinces => provinces;
	
	public void Init(CountryData data, MapGraph map){
		gameObject.name = data.Name;
		MapColor = data.MapColor;
		foreach (Color32 province in data.Provinces){
			map[province].SetOwner(this);
		}
		Color borderColor = MapColor*borderBrightnessFactor;
		borderColor.a = 1;
		borderMeshRenderer.material.color = borderColor;
		RegenerateBorder();
	}
	private void RegenerateBorder(){
		DestroyImmediate(borderMeshFilter.sharedMesh);
		MeshData borderMeshData = new($"{gameObject.name}BorderMesh");
		// TODO: Only add the sections of vertices between outer border tri-points.
		foreach (Province province in provinces){
			List<Vector2> borderVertices = new();
			for (int i = 0; i < province.TriPointIndices.Count; i++){
				int startIndex = province.TriPointIndices[i];
				int endIndex = province.TriPointIndices[(i+1)%province.TriPointIndices.Count];
				for (int j = startIndex; j != endIndex; j = (j+1)%province.Vertices.Count){
					borderVertices.Add(province.MapPosition+province.Vertices[j]);
				}
			}
			PolygonOutline.GenerateMeshData(borderMeshData, borderVertices, borderHalfWidth);
		}
		borderMeshFilter.mesh = borderMeshData.ToMesh();
		wasBorderChanged = false;
	}
	private void Update(){
		if (wasBorderChanged){
			RegenerateBorder();
		}
	}
	public bool LoseProvince(Province province){
		bool didLoseProvince = provinces.Remove(province);
		wasBorderChanged |= didLoseProvince;
		return didLoseProvince;
	}
	public bool GainProvince(Province province){
		bool didGainProvince = provinces.Add(province);
		wasBorderChanged |= didGainProvince;
		return didGainProvince;
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
