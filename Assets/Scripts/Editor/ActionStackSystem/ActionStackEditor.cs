using UnityEngine;
using UnityEditor;

namespace ActionStackSystem {
	[CustomEditor(typeof(ActionStack<>), true)]
	public class ActionStackEditor : Editor {
		public override void OnInspectorGUI(){
			base.OnInspectorGUI();
			IReadOnlyActionStack<IAction> stack = target as IReadOnlyActionStack<IAction>;
			Debug.Assert(stack != null);
			
			GUILayout.Space(10);
			EditorGUILayout.LabelField("Action Stack", EditorStyles.boldLabel);
			GUILayout.BeginVertical(EditorStyles.helpBox);
			int i = 0;
			foreach (IAction action in stack.Actions){
				string actionName = $"   #{i}: {action}";
				GUIStyle labelStyle = action == stack.CurrentAction ? EditorStyles.boldLabel : EditorStyles.label;
				if (action is Object obj){
					if (GUILayout.Button(actionName, labelStyle)){
						Selection.activeObject = obj;
					}
				} else {
					EditorGUILayout.LabelField(actionName, labelStyle);
				}
				i++;
			}
			GUILayout.EndVertical();
		}
	}
}