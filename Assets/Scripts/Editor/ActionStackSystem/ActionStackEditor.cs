using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ActionStackSystem {
	[CustomEditor(typeof(ActionStack), true)]
	public class ActionStackEditor : Editor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			ActionStack stack = target as ActionStack;
			Debug.Assert(stack != null);
			
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Action Stack", EditorStyles.boldLabel);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			foreach (ActionStack.IAction action in stack.StackList){
				string actionName = $"   #{stack.StackList.IndexOf(action)}: {action}";
				GUIStyle labelStyle = action == stack.CurrentAction ? EditorStyles.boldLabel : EditorStyles.label;
				if (action is Object obj){
					if (GUILayout.Button(actionName, labelStyle)){
						Selection.activeObject = obj;
					}
				} else {
					EditorGUILayout.LabelField(actionName, labelStyle);
				}
			}
			GUILayout.EndVertical();
		}
	}
}