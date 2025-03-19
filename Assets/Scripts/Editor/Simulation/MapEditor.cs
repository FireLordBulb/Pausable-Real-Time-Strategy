using System.Collections.Generic;
using Graphs;
using UnityEditor;

[CustomEditor(typeof(MapGraph), true)]
public class MapEditor : Editor {
	private IEnumerable<Province> nodes;
	private void OnEnable(){
		nodes = ((MapGraph)target).Nodes;
	}
	
	private void OnSceneGUI(){
		EditorGraphUtils<Province, ProvinceLink>.DrawGraph(nodes);
	}
}
