using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Countries", menuName = "ScriptableObjects/MapSetup/Countries")]
public class Countries : ScriptableObject {
	[SerializeField] private CountryData[] countries;
	public IEnumerable<CountryData> List => countries;
}

[Serializable]
public class CountryData {
	[SerializeField] private string name;
	[SerializeField] private Color mapColor;
}
