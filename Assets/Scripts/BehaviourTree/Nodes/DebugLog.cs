using UnityEngine;

namespace BehaviourTree.Nodes {
	public class DebugLog : ActionNode {
		public string message;

		#region Properties
		public override string Description => message;
		#endregion

		protected override void OnStart(){
			Debug.Log($"OnStart {message}");
		}
		protected override void OnStop(){
			Debug.Log($"OnStop {message}");
		}
		protected override State OnUpdate(){
			Debug.Log($"OnUpdate {message}");
			return State.Success;
		}
	}
}
