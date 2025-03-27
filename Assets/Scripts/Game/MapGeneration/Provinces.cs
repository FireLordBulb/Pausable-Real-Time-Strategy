using System.Collections.Generic;
using Simulation;
using UnityEngine;

namespace MapGeneration {
	[CreateAssetMenu(fileName = "Provinces", menuName = "ScriptableObjects/MapSetup/Provinces")]
	public class Provinces : ScriptableObject {
		[SerializeField] private ProvinceData[] provinces;
		public IEnumerable<ProvinceData> List => provinces;
	}
}
