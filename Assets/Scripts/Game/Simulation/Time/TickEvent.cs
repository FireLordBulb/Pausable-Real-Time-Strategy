using System;
using System.Collections.Generic;

namespace Simulation {
	public class TickEvent {
		private readonly SortedSet<TypedCallback> callbacks;
		private readonly Queue<(TypedCallback, bool)> callbackChangeBuffer = new();

		internal TickEvent(CallbackSorter sorter){
			callbacks = new SortedSet<TypedCallback>(sorter);
		}
		
		// Always pass this.GetType() into the callerType parameter.
		public void AddListener(Action callback, Type callerType){
			callbackChangeBuffer.Enqueue((new TypedCallback {Value = callback, Type = callerType}, true));
		}
		public void RemoveListener(Action callback){
			callbackChangeBuffer.Enqueue((new TypedCallback {Value = callback}, false));
		}
		public void Invoke(){
			while (callbackChangeBuffer.Count > 0){
				(TypedCallback callback, bool doAdd) = callbackChangeBuffer.Dequeue();
				if (doAdd){
					callbacks.Add(callback);
				} else {
					callbacks.Remove(callback);
				}
			}
			foreach (TypedCallback callback in callbacks){
				callback.Value();
			}
		}
	}
}