namespace BehaviourTree.Nodes {
	public abstract class DecoratorNode : Node {
		public Node child;

		#region Properties
		#endregion

		protected override void OnStart(){}
		protected override void OnStop(){}
		protected override State OnUpdate(){
			return child.Update();
		}
		public override Node Clone(){
			DecoratorNode clone = (DecoratorNode)base.Clone();
			if (child != null){
				clone.child = child.Clone();
			}
			return clone;
		}
	}
}
