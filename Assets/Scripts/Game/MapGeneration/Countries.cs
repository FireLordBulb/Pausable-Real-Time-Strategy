using System.Collections.Generic;
using Simulation;
using UnityEngine;

namespace MapGeneration {
	[CreateAssetMenu(fileName = "Countries", menuName = "ScriptableObjects/MapSetup/Countries")]
	public class Countries : ScriptableObject {
		[SerializeField] private CountryData[] countries;
		public IEnumerable<CountryData> List => countries;
	}
}
