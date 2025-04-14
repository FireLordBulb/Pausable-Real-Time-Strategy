using BehaviourTree.Nodes;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTree {
	public class TreeEditor : EditorWindow {
		[SerializeField] private VisualTreeAsset visualTreeAsset;
		private TreeView view;
		[OnOpenAsset]
		public static bool OpenAITree(int instanceID, int line){
			Tree tree = EditorUtility.InstanceIDToObject(instanceID) as Tree;
			Node node = EditorUtility.InstanceIDToObject(instanceID) as Node;
			if (tree == null && node != null){
				tree = AssetDatabase.LoadAssetAtPath<Tree>(AssetDatabase.GetAssetPath(instanceID));
			}
			if (tree == null){
				return false;
			}
			TreeEditor treeWindow = GetWindow<TreeEditor>();
			Selection.activeObject = tree;
			treeWindow.titleContent = new GUIContent(tree.name);
			return true;
		}
		private void OnEnable(){
			EditorApplication.playModeStateChanged -= PlayModeChanged;
			EditorApplication.playModeStateChanged += PlayModeChanged;
		}
		private void OnDestroy(){
			EditorApplication.playModeStateChanged -= PlayModeChanged;
		}
		private void PlayModeChanged(PlayModeStateChange obj){
			CreateGUI();
			view?.PopulateView(null);
			titleContent = new GUIContent("BT");
		}
		public void CreateGUI(){
			if (view == null){
				VisualElement root = rootVisualElement;
				visualTreeAsset.CloneTree(root);
				view = root.Query<TreeView>();
			}
			OnSelectionChange();
		}
		private void OnSelectionChange(){
			Tree tree = Selection.activeObject as Tree;

			if (tree == null && Application.isPlaying && Selection.activeGameObject != null){
				Brain brain = Selection.activeGameObject.GetComponent<Brain>();
				if (brain != null){
					tree = brain.Tree;
				}
			}

			if (tree != null && (AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()) || Application.isPlaying)){
				view.PopulateView(tree);
				titleContent = new GUIContent(tree.name);
			}
		}
		private void OnInspectorUpdate(){
			view?.UpdateNodeStates();
		}
	}
}
