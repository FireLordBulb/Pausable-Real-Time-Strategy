namespace BehaviourTree.Nodes {
	public class Repeat : DecoratorNode {
		protected override State OnUpdate(){
			if (child != null){
				child.Update();
			}
			return State.Running;
		}
	}
}
