using UnityEngine;

namespace Player {
	[RequireComponent(typeof(Simulation.Military.IUnit))]
	public class MilitaryUnitVisuals : MonoBehaviour {
		[SerializeField] private MeshRenderer[] renderers;
		private void Start(){
			Color color = GetComponent<Simulation.Military.IUnit>().Owner.MapColor;
			foreach (MeshRenderer meshRenderer in renderers){
				meshRenderer.material.color = color;
			}
		}
	}
}