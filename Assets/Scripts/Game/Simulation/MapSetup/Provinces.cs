using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Provinces", menuName = "ScriptableObjects/MapSetup/Provinces")]
public class Provinces : ScriptableObject {
	[SerializeField] private ProvinceData[] provinces;
	public ProvinceData this[int index] => provinces[index];
}

[Serializable]
public class ProvinceData {
	[SerializeField] private Color32 color;
}
