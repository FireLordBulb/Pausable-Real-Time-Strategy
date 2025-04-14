using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using System;
using System.Linq;
using BehaviourTree.Nodes;

namespace BehaviourTree {
	public class TreeView : GraphView {
		public new class UxmlFactory : UxmlFactory<TreeView, UxmlTraits> {}

		private Tree tree;

		public TreeView(){
			Insert(0, new GridBackground());
			this.AddManipulator(new ContentZoomer());
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Editor/BehaviourTree/TreeEditor.uss");
			styleSheets.Add(styleSheet);
			Undo.undoRedoPerformed += OnUndoRedo;
		}
		private void OnUndoRedo(){
			PopulateView(tree);
			AssetDatabase.SaveAssets();
		}
		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt){
			TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<Nodes.Node>();
			foreach (Type type in types){
				if (!type.IsAbstract && type != typeof(Root)){
					evt.menu.AppendAction("Create Node/"+type.Name, (a) => CreateNode(type));
				}
			}
		}
		private void CreateNode(Type type){
			if (tree != null){
				Nodes.Node node = tree.CreateNode(type);
				CreateNodeControl(node);
			}
		}
		internal void PopulateView(Tree treeReference){
			tree = treeReference;
			graphViewChanged -= OnGraphViewChanged;
			DeleteElements(graphElements);
			graphViewChanged += OnGraphViewChanged;
			if (tree == null){
				return;
			}
			if (tree.root == null){
				tree.root = tree.CreateNode(typeof(Root)) as Root;
				EditorUtility.SetDirty(tree);
				AssetDatabase.SaveAssetIfDirty(tree);
			}

			tree.nodes.ForEach(CreateNodeControl);

			tree.nodes.ForEach(n => {
				List<Nodes.Node> children = Tree.GetChildren(n);
				NodeControl parentControl = FindNodeControl(n);
				children.ForEach(c => {
					NodeControl childControl = FindNodeControl(c);
					Edge edge = parentControl.Output.ConnectTo(childControl.Input);
					AddElement(edge);
				});
			});
		}
		private void CreateNodeControl(Nodes.Node node){
			if (node != null){
				NodeControl snc = new(node);
				AddElement(snc);
			}
		}
		private NodeControl FindNodeControl(Nodes.Node node){
			return GetNodeByGuid(node.ID.ToString()) as NodeControl;
		}
		public void UpdateNodeStates(){
			nodes.ForEach(n => {
				if (n is NodeControl snc){
					snc.UpdateState();
				}
			});
		}
		private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange){
			graphViewChange.elementsToRemove?.ForEach(e => {
				switch(e){
					case NodeControl nodeControl:
						tree.DeleteNode(nodeControl.Node);
						break;
					case Edge edge: 
						NodeControl from = (NodeControl)edge.output.node;
						NodeControl to = (NodeControl)edge.input.node;
						tree.RemoveChild(from.Node, to.Node);
						break;
				}
			});
			graphViewChange.edgesToCreate?.ForEach(edge => {
				NodeControl from = (NodeControl)edge.output.node;
				NodeControl to = (NodeControl)edge.input.node;
				tree.AddChild(from.Node, to.Node);
			});

			if (graphViewChange.movedElements == null || tree == null){
				return graphViewChange;
			}
			foreach (Nodes.Node node in tree.nodes){
				if (node is CompositeNode compositeNode){
					compositeNode.SortChildren();
				}
			}
			return graphViewChange;
		}
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter){
			return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
		}
	}
}
