namespace BehaviourTree.Nodes {
	public class Selector : CompositeNode {
		private int currentChildIndex;
		protected override void OnStart(){
			currentChildIndex = 0;
		}
		protected override void OnStop(){}
		protected override State OnUpdate(){
			if (children.Count == 0){
				return State.Failure;
			}
			do {
				Node node = children[currentChildIndex];
				switch(node.Update()){
					case State.Running:
						return State.Running;
					case State.Success:
						return State.Success;
					case State.Failure:
						currentChildIndex++;
						break;
				}
				if (currentChildIndex == children.Count){
					return State.Failure;
				}
			} while (doCheckMultipleInSingleUpdate);
			return State.Running;
		}
	}
}
