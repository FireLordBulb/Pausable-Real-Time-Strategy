using System.Collections.Generic;
using UnityEngine;
using BehaviourTree.Nodes;

namespace BehaviourTree {
	[CreateAssetMenu(fileName = "BehaviourTree", menuName = "AICourse/BehaviourTree")]
	public class Tree : ScriptableObject {
		[SerializeField] public List<Node> nodes = new();
		[SerializeField] public Node root;
		private Brain targetBrain;
		private Node.State currentState = Node.State.Running;
		private Blackboard blackboard;

		#region Properties
		public Brain TargetBrain => targetBrain;
		public GameObject TargetGameObject => targetBrain.gameObject;
		public Blackboard Blackboard => blackboard;
		public Node.State CurrentState => currentState;
		#endregion

		public void StartTree(Brain brain){
			targetBrain = brain;
			blackboard = new Blackboard();
			foreach (Node node in nodes){
				node.OnTreeStart(this);
			}
		}
		public virtual Node.State Update(){
			if (root != null && root.CurrentState == Node.State.Running){
				currentState = root.Update();
			}
			return currentState;
		}
		public Node CreateNode(System.Type type){
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(this, "State Created");
#endif
			Node node = (Node)CreateInstance(type);
			node.name = type.Name;
			nodes.Add(node);
#if UNITY_EDITOR
			UnityEditor.AssetDatabase.AddObjectToAsset(node, this);
			UnityEditor.Undo.RegisterCreatedObjectUndo(node, "State Created");
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
			return node;
		}
		public void DeleteNode(Node node){
			if (node == null){
				return;
			}
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(this, "State Deleted");
#endif
			nodes.Remove(node);
#if UNITY_EDITOR
			UnityEditor.Undo.DestroyObjectImmediate(node);
			UnityEditor.EditorUtility.SetDirty(this);
			UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
#endif
		}
		public void AddChild(Node parent, Node child){
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(parent, "Node Child Added");
#endif
			if (parent is Root rootNode){
				rootNode.child = child;
			} else if (parent is DecoratorNode decorator){
				decorator.child = child;
			} else if (parent is CompositeNode composite &&
			           !composite.children.Contains(child)){
				composite.children.Add(child);
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(parent);
#endif
		}
		public void RemoveChild(Node parent, Node child){
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(parent, "Node Child Removed");
#endif
			if (parent is Root rootNode){
				rootNode.child = null;
			} else if (parent is DecoratorNode decorator){
				decorator.child = null;
			} else if (parent is CompositeNode composite && composite.children.Contains(child)){
				composite.children.Remove(child);
			}
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(parent);
#endif
		}
		public static List<Node> GetChildren(Node parent){
			List<Node> children = new();
			if (parent is Root rootNode){
				children.Add(rootNode.child);
			} else if (parent is DecoratorNode decorator){
				children.Add(decorator.child);
			} else if (parent is CompositeNode composite){
				children.AddRange(composite.children);
			}
			children.RemoveAll(c => c == null);
			return children;
		}
		public void Traverse(Node node, System.Action<Node> callback){
			if (node == null){
				return;
			}
			callback(node);
			List<Node> children = GetChildren(node);
			children.ForEach(n => Traverse(n, callback));
		}
		public Tree Clone(){
			Tree clone = Instantiate(this);
			clone.name = clone.name.Replace("(Clone)", " (Runtime)");

			clone.root = root.Clone();
			clone.nodes = new List<Node>();
			Traverse(clone.root, (n) => {clone.nodes.Add(n);});
			return clone;
		}
	}
}