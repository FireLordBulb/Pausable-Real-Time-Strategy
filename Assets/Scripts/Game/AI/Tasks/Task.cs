using System;
using UnityEngine;

namespace AI {
	public abstract class Task : ScriptableObject, IComparable<Task> {
		protected AIController Controller;
		
		private int priority;

		public void Init(AIController controller){
			Controller = controller;
		}
		public int CompareTo(Task otherTask){
			return otherTask.priority-priority;
		}
		public void RecalculatePriority(){
			priority = CurrentPriority();
		}

		protected abstract int CurrentPriority();
		public abstract bool CanBePerformed();
		public abstract void Perform();
	}
}