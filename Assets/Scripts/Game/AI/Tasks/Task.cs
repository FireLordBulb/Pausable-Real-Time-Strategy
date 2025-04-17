using System;
using Simulation;
using UnityEngine;

namespace AI {
	public abstract class Task : ScriptableObject, IComparable<Task> {
		[SerializeField] protected int defaultPriority;
		
		protected AIController Controller;
		
		private int priority;
		protected int Priority => priority;
		protected Country Country => Controller.Country;

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