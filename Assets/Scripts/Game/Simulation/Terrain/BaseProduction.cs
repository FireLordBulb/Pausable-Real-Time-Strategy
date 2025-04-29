using UnityEngine;

namespace Simulation {
	[CreateAssetMenu(fileName = "BaseProduction", menuName = "ScriptableObjects/BaseProduction")]
	public class BaseProduction : ScriptableObject {
		[SerializeField] private float gold;
		[SerializeField] private float manpower;
		[SerializeField] private float sailors;

		public float Gold => gold;
		public float Manpower => manpower;
		public float Sailors => sailors;
	}
}