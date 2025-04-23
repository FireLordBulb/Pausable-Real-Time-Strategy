using System.Collections.Generic;
using UnityEngine;

namespace Simulation {
	[RequireComponent(typeof(Province))]
	public class Sea : MonoBehaviour {
		[SerializeField] private Color color;
		[SerializeField] private Terrain terrain;

		public Province Province {get; private set;}
		public Military.SeaLocation NavyLocation {get; private set;}

		public void Init(Color32 colorKey, MapGraph mapGraph, Vector2 mapPosition, Mesh outlineMesh, Mesh shapeMesh, IEnumerable<Vector2> vertices){
			NavyLocation = new Military.SeaLocation(this);
			Province = GetComponent<Province>();
			Province.Init(colorKey, mapGraph, terrain, color, mapPosition, outlineMesh, shapeMesh, vertices);
		}
	}
}
