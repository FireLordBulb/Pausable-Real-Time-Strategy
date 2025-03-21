using System;
using System.Collections.Generic;
using UnityEngine;

public class Country : MonoBehaviour {
	private readonly List<Province> provinces = new();
	
	public Color MapColor {get; private set;}
	public void Init(CountryData data, MapGraph map){
		gameObject.name = data.Name;
		MapColor = data.MapColor;
		foreach (Color32 province in data.Provinces){
			map[province].SetOwner(this);
		}
	}
	
	public bool LoseProvince(Province province) => provinces.Remove(province);
	public void GainProvince(Province province) => provinces.Add(province);
	//private readonly List<Province> provinces = new();
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
