using System;
using System.Collections.Generic;
using UnityEngine;

public class Country : MonoBehaviour {
	private Color mapColor;
	private readonly List<Province> provinces = new();
	public void Init(CountryData data, MapGraph map){
		gameObject.name = name = data.Name;
		foreach (Color32 province in data.Provinces){
			provinces.Add(map[province]);
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
