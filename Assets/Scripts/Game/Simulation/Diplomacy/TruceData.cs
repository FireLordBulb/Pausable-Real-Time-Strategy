using UnityEngine;

namespace Simulation {
	[CreateAssetMenu(fileName = "TruceData", menuName = "ScriptableObjects/TruceData", order = 0)]
	public class TruceData : ScriptableObject {
		[SerializeField] private int baseTruceDays;
		[SerializeField] private int minTruceDays;
		[SerializeField] private int maxTruceDays;
		[SerializeField] private float daysPerProvince;
		[SerializeField] private float daysPerDevelopment;
		[SerializeField] private float daysPerGold;

		public int BaseTruceDays => baseTruceDays;
		public int MinTruceDays => minTruceDays;
		public int MaxTruceDays => maxTruceDays;
		public float DaysPerProvince => daysPerProvince;
		public float DaysPerDevelopment => daysPerDevelopment;
		public float DaysPerGold => daysPerGold;
	}
}