using UnityEngine;

namespace Simulation {
	[CreateAssetMenu(fileName = "TruceData", menuName = "ScriptableObjects/TruceData", order = 0)]
	public class TruceData : ScriptableObject {
		[SerializeField] private int minTruceDays;
		[SerializeField] private float daysPerProvince;
		[SerializeField] private float daysPerDevelopment;
		[SerializeField] private float daysPerGold;

		public int MinTruceLength => minTruceDays;
		public float DaysPerProvince => daysPerProvince;
		public float DaysPerDevelopment => daysPerDevelopment;
		public float DaysPerGold => daysPerGold;
	}
}