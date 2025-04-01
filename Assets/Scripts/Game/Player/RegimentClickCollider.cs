using UnityEngine;

namespace Player {
	public class RegimentClickCollider : MonoBehaviour {
		[SerializeField] private Simulation.Military.Regiment regiment;
		public Simulation.Military.Regiment Regiment => regiment;
	}
}