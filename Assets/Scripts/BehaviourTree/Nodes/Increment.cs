namespace BehaviourTree.Nodes {
	public class Increment : DecoratorNode {
		public string key = "VariableName";

		#region Properties
		public override string Description => "Increment "+key;
		#endregion

		protected override void OnStart(){
			if (Tree != null && Tree.Blackboard != null){
				int iValue = Tree.Blackboard.GetValue(key, 0);
				Tree.Blackboard.SetValue(key, iValue+1);
			}
		}
	}
}
