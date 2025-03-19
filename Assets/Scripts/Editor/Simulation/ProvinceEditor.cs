using System.Collections.Generic;
using Graphs;
using UnityEditor;

[CustomEditor(typeof(Province), true)]
public class ProvinceEditor : Editor {
	private IEnumerable<Province> nodes;
	private void OnEnable(){
		nodes = new[]{(Province)target};
	}
	private void OnSceneGUI(){
		EditorGraphUtils<Province, ProvinceLink>.DrawGraph(nodes);
	}
}
