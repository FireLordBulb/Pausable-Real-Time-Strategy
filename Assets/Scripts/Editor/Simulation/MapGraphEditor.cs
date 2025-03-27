using System.Collections.Generic;
using Graphs;
using UnityEditor;

namespace Simulation {
	public abstract class MapGraphEditor : Editor {
		protected IEnumerable<Province> Nodes;
		protected virtual void OnSceneGUI(){
			EditorGraphUtils<Province, ProvinceLink>.DrawGraph(Nodes);
		}
	}
}
