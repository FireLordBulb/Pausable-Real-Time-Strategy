using System.Collections.Generic;
using Graphs;
using UnityEditor;
using UnityEngine;

namespace Simulation {
	public abstract class MapGraphEditor : Editor {
		private static readonly Dictionary<System.Type, Color> LinkColors = new(){
			{typeof(LandLink), Color.magenta},
			{typeof(CoastLink), Color.green},
			{typeof(SeaLink), Color.red}
		};
		protected IEnumerable<Province> Nodes;
		protected virtual void OnSceneGUI(){
			EditorGraphUtils<Province, ProvinceLink>.DrawGraph(Nodes, LinkColors);
		}
	}
}
