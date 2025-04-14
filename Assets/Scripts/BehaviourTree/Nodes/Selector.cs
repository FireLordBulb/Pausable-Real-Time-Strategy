namespace BehaviourTree.Nodes {
	public class Selector : CompositeNode {
		private int currentChild;
		protected override void OnStart(){
			currentChild = 0;
		}
		protected override void OnStop(){}
		protected override State OnUpdate(){
			if (children.Count == 0){
				return State.Failure;
			}
			Node node = children[currentChild];
			switch(node.Update()){
				case State.Running:
					return State.Running;
				case State.Success:
					return State.Success;
				case State.Failure:
					currentChild++;
					break;
			}
			return currentChild == children.Count ? State.Failure : State.Running;
		}
	}
}
