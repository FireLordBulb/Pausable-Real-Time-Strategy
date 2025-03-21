using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Provinces", menuName = "ScriptableObjects/MapSetup/Provinces")]
public class Provinces : ScriptableObject {
	[SerializeField] private ProvinceData[] provinces;
	public IEnumerable<ProvinceData> List => provinces;
}
