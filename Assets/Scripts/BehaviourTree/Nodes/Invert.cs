namespace BehaviourTree.Nodes {
	public class Invert : DecoratorNode {
		protected override State OnUpdate(){
			switch(child.Update()){
				case State.Success:
					return State.Failure;
				case State.Failure:
					return State.Success;
				default:
					return State.Running;
			}
		}
	}
}
