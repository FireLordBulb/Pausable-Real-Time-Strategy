using System.Collections.Generic;
using Graphs;
using UnityEditor;

[CustomEditor(typeof(Province), true)]
public class ProvinceEditor : MapGraphEditor {
	private void OnEnable(){
		Nodes = new[]{(Province)target};
	}
}
