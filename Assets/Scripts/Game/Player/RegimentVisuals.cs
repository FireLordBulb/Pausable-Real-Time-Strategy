using Simulation.Military;
using UnityEngine;

namespace Player {
	[RequireComponent(typeof(Regiment))]
	public class RegimentVisuals : MonoBehaviour {
		[SerializeField] private MeshRenderer[] renderers;
		private void Start(){
			Color color = GetComponent<Regiment>().Owner.MapColor;
			foreach (MeshRenderer meshRenderer in renderers){
				meshRenderer.material.color = color;
			}
		}
	}
}