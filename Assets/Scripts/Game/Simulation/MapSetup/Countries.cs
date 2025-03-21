using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Countries", menuName = "ScriptableObjects/MapSetup/Countries")]
public class Countries : ScriptableObject {
	[SerializeField] private CountryData[] countries;
	public CountryData this[int index] => countries[index];
}

[Serializable]
public class CountryData {
	[SerializeField] private string name;
	[SerializeField] private Color mapColor;
}
