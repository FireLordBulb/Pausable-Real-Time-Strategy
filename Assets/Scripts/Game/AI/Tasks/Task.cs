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

		internal void Init(AIController controller){
			Controller = controller;
		}
		public int CompareTo(Task otherTask){
			return otherTask.priority-priority;
		}
		internal void RecalculatePriority(){
			priority = CurrentPriority();
		}

		protected abstract int CurrentPriority();
		internal abstract bool CanBePerformed();
		internal abstract void Perform();
	}
}