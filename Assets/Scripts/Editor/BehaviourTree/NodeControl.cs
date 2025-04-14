using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using BehaviourTree.Nodes;

namespace BehaviourTree {
	public sealed class NodeControl : UnityEditor.Experimental.GraphView.Node {
		private readonly Label description;

		public Nodes.Node Node {get;}
		public Port Input {get; private set;}
		public Port Output {get; private set;}

		public NodeControl(Nodes.Node node) : base("Assets/Editor/AI/NodeControl.uxml"){
			Node = node;
			title = node.name;
			viewDataKey = node.ID.ToString();
			style.left = node.position.x;
			style.top = node.position.y;
			CreatePorts();
			if (Node is ActionNode){
				AddToClassList("action");
			} else if (Node is CompositeNode){
				AddToClassList("composite");
			} else if (Node is Root){
				AddToClassList("root");
			} else if (Node is DecoratorNode){
				AddToClassList("decorator");
			}
			description = this.Q<Label>("description");
			description.text = Node.Description;
		}
		public override void SetPosition(Rect newPos){
			base.SetPosition(newPos);
			Undo.RecordObject(Node, "Moving State");
			Node.position = new Vector2(newPos.xMin, newPos.yMin);
			EditorUtility.SetDirty(Node);
		}
		private void CreatePorts(){
			if (Node is not Root){
				Input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
				Input.portName = "";
				Input.style.flexDirection = FlexDirection.Column;
				inputContainer.Add(Input);
			}

			if (Node is CompositeNode){
				Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
			} else if (Node is DecoratorNode or Root){
				Output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
			}
			if (Output == null){
				return;
			}
			Output.portName = "";
			Output.style.flexDirection = FlexDirection.ColumnReverse;
			outputContainer.Add(Output);
		}
		public override void OnSelected(){
			base.OnSelected();
			Selection.activeObject = Node;
		}
		public void UpdateState(){
			description.text = Node.Description;
			if (!Application.isPlaying){
				return;
			}
			RemoveFromClassList("running");
			RemoveFromClassList("success");
			RemoveFromClassList("failure");
			switch(Node.CurrentState){
				case Nodes.Node.State.Running:
					if (Node.IsStarted){
						AddToClassList("running");
					}
					break;
				case Nodes.Node.State.Success:
					AddToClassList("success");
					break;
				case Nodes.Node.State.Failure:
					AddToClassList("failure");
					break;
			}
		}
	}
}
