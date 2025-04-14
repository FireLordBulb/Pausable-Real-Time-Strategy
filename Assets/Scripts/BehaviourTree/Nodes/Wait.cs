using UnityEngine;

namespace BehaviourTree.Nodes {
	public class Wait : ActionNode {
		public float time = 1.0f;
		private float elapsedTime;

		#region Properties
		private float RemainingTime => IsStarted && CurrentState == State.Running ? time-elapsedTime : 0.0f;
		public override string Description => RemainingTime.ToString("0.00")+" sec";
		#endregion

		protected override void OnStart(){
			elapsedTime = 0.0f;
		}
		protected override void OnStop(){}
		protected override State OnUpdate(){
			elapsedTime += Time.deltaTime;
			return RemainingTime < 0.0f ? State.Success : State.Running;
		}
	}
}
