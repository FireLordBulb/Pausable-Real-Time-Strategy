using BehaviourTree.Nodes;
using Simulation;
using UnityEngine;

namespace AI {
	[CreateAssetMenu(fileName = "TickTree", menuName = "ScriptableObjects/AI/TickTree")]
	public class TickTree : BehaviourTree.Tree {
		private Calendar calendar;
		
		public void Init(Calendar calendarReference){
			calendar = calendarReference;
		}
		public void Enable(){
			calendar.OnDayTick.AddListener(DayTick);
		}
		public void Disable(){
			calendar.OnDayTick.RemoveListener(DayTick);
		}
		// Don't update the nodes in regular Update.
		public override Node.State Update(){
			return CurrentState;
		}
		private void DayTick(){
			base.Update();
		}
		private void OnDestroy(){
			calendar.OnDayTick.RemoveListener(DayTick);
		}
	}
}
