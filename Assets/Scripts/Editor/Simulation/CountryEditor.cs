using System.Collections.Generic;
using Graphs;
using UnityEditor;

[CustomEditor(typeof(Country), true)]
public class CountryEditor : MapGraphEditor {
	private void OnEnable(){
		Nodes = ((Country)target).Provinces;
	}
}
