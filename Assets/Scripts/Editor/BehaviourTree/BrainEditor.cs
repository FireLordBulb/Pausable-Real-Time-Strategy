using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BehaviourTree {
	[CustomEditor(typeof(Brain), true)]
	public class BrainEditor : Editor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();

			Brain brain = (Brain)target;
			if (brain.Tree == null || !Application.isPlaying || brain.Tree.Blackboard == null){
				return;
			}
			Blackboard blackboard = brain.Tree.Blackboard;
			GUILayout.Space(10);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField("Blackboard ("+blackboard.Count+")", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;

			foreach (KeyValuePair<string, object> kvp in blackboard.Items){
				if (kvp.Value == null){
					EditorGUILayout.LabelField(kvp.Key, "NULL");
				} else if (kvp.Value is IEnumerable objects){
					EditorGUI.indentLevel++;
					GUILayout.BeginVertical(EditorStyles.helpBox);
					EditorGUILayout.LabelField(kvp.Key, objects.GetType().Name);
					foreach (object obj in objects){
						GUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel("");
						EditorGUILayout.LabelField(obj.ToString());
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
					EditorGUI.indentLevel--;
				} else {
					EditorGUILayout.LabelField(kvp.Key, kvp.Value.ToString());
				}
			}
			EditorGUI.indentLevel--;
			GUILayout.EndVertical();
			Repaint();
		}
	}
}
