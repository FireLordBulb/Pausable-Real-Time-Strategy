using System.Collections.Generic;
using Graphs;
using UnityEditor;

[CustomEditor(typeof(MapGraph), true)]
public class MapEditor : MapGraphEditor {
	private void OnEnable(){
		Nodes = ((MapGraph)target).Nodes;
	}
}
