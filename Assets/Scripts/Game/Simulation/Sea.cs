using UnityEngine;

namespace Simulation {
	[RequireComponent(typeof(Province))]
	public class Sea : MonoBehaviour {
		[SerializeField] private Color color;
		[SerializeField] private Terrain terrain;

		public Province Province {get; private set;}

		public void Init(Color32 colorKey, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh){
			Province = GetComponent<Province>();
			Province.Init(colorKey, Province.Type.Sea, terrain, color, mapPosition, outlineMesh, shapeMesh);
		}
	}
}
