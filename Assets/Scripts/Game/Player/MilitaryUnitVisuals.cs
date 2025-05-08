using UnityEngine;

namespace Player {
	[RequireComponent(typeof(Simulation.Military.IUnit))]
	public class MilitaryUnitVisuals : MonoBehaviour {
		[SerializeField] private Renderer[] renderers;
		private void Start(){
			Color color = GetComponent<Simulation.Military.IUnit>().Owner.MapColor;
			foreach (Renderer recolorableRenderer in renderers){
				recolorableRenderer.materials[^1].color = color;
			}
		}
	}
}
