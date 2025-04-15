using System;
using System.Collections.Generic;

namespace Simulation {
	public class TickEvent {
		private readonly SortedSet<Action> callbacks;
		private readonly Queue<(Action, bool)> callbackChangeBuffer = new();
		private bool isIterating;
		
		internal TickEvent(IComparer<Action> sorter){
			callbacks = new SortedSet<Action>(sorter);
		}
		
		// ReSharper disable Unity.PerformanceAnalysis
		public void AddListener(Action callback){
			if (isIterating){
				callbackChangeBuffer.Enqueue((callback, true));
			} else {
				callbacks.Add(callback);
			}
		}
		// ReSharper disable Unity.PerformanceAnalysis
		public void RemoveListener(Action callback){
			if (isIterating){
				callbackChangeBuffer.Enqueue((callback, false));
			} else {
				callbacks.Remove(callback);
			}
		}
		internal void Invoke(){
			GoThroughBuffer();
			isIterating = true;
			foreach (Action callback in callbacks){
				callback();
			}
			isIterating = false;
			GoThroughBuffer();
		}
		private void GoThroughBuffer(){
			while (callbackChangeBuffer.Count > 0){
				(Action callback, bool doAdd) = callbackChangeBuffer.Dequeue();
				if (doAdd){
					callbacks.Add(callback);
				} else {
					callbacks.Remove(callback);
				}
			}
		}
	}
}
