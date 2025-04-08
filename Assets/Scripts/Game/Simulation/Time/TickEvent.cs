using System;
using System.Collections.Generic;

namespace Simulation {
	public class TickEvent {
		private readonly SortedSet<Action> callbacks;
		private readonly Queue<(Action, bool)> callbackChangeBuffer = new();

		internal TickEvent(IComparer<Action> sorter){
			callbacks = new SortedSet<Action>(sorter);
		}
		
		public void AddListener(Action callback, Type callerType){
			callbackChangeBuffer.Enqueue((callback, true));
		}
		public void RemoveListener(Action callback){
			callbackChangeBuffer.Enqueue((callback, false));
		}
		public void Invoke(){
			while (callbackChangeBuffer.Count > 0){
				(Action callback, bool doAdd) = callbackChangeBuffer.Dequeue();
				if (doAdd){
					callbacks.Add(callback);
				} else {
					callbacks.Remove(callback);
				}
			}
			foreach (Action callback in callbacks){
				callback();
			}
		}
	}
}
