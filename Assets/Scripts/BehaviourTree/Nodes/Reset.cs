namespace BehaviourTree.Nodes {
	public class Reset : DecoratorNode {
		protected override void OnStop(){
			base.OnStop();
			if (CurrentState == State.Failure){
				Tree.Traverse(this, node => {
					if (node != this){
						node.IsStarted = false;
						node.CurrentState = State.Running;
					}
				});
			}
		}
		protected override State OnUpdate(){
			if (child != null){
				CurrentState = child.Update();
			}
			return CurrentState;
		}
	}
}
