namespace BehaviourTree.Nodes {
	public class Sequence : CompositeNode {
		private int currentChildIndex;
		protected override void OnStart(){
			currentChildIndex = 0;
		}
		protected override void OnStop(){}
		protected override State OnUpdate(){
			if (children.Count == 0){
				return State.Success;
			}
			Node node = children[currentChildIndex];
			switch(node.Update()){
				case State.Running:
					return State.Running;
				case State.Success:
					currentChildIndex++;
					break;
				case State.Failure:
					return State.Failure;
			}
			return currentChildIndex == children.Count ? State.Success : State.Running;
		}
	}
}
