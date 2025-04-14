namespace BehaviourTree.Nodes {
	public class Root : Node {
		public Node child;
		protected override void OnStart(){}
		protected override void OnStop(){}
		protected override State OnUpdate(){
			if (child != null){
				return child.Update();
			}
			return State.Success;
		}
		public override Node Clone(){
			Root clone = (Root)base.Clone();
			clone.child = child.Clone();
			return clone;
		}
	}
}
