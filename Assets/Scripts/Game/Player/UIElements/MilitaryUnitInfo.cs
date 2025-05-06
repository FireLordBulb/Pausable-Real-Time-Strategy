using Simulation.Military;
using UnityEngine;

namespace Player {
	[RequireComponent(typeof(RectTransform))]
	public class MilitaryUnitInfo : MonoBehaviour {
		public RectTransform RectTransform => (RectTransform)transform;
		
		public void Init(IUnit unit){
			
		}
	}
}