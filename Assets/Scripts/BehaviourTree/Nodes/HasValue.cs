namespace BehaviourTree.Nodes {
	public class HasValue : DecoratorNode {
		public string key = "VariableName";
		public bool doInvert;
		public bool doCheckEveryFrame = true;
		private bool resultFlag;

		#region Properties
		public override string Description =>
			(doInvert ? "Don't have " : "Has ")+key+(doCheckEveryFrame ? " (EF)" : "");
		#endregion

		protected override void OnStart(){
			base.OnStart();
			if (!doCheckEveryFrame){
				UpdateResult();
			}
		}
		private void UpdateResult(){
			resultFlag = Tree != null && Tree.Blackboard != null && Tree.Blackboard.ContainsKey(key) != doInvert;
		}
		protected override State OnUpdate(){
			if (doCheckEveryFrame){
				UpdateResult();
			}
			if (!resultFlag){
				CurrentState = State.Failure;
			} else if (child != null){
				CurrentState = child.Update();
			}
			return CurrentState;
		}
	}
}
