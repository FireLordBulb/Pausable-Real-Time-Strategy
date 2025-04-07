using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation {
	internal class CallbackSorter : IComparer<TypedCallback> {
		private readonly Dictionary<Type, int> priority = new();
		
		public CallbackSorter(string[] orderedTypes){
			int halfLength = orderedTypes.Length/2;
			for (int i = 0; i < orderedTypes.Length; i++){
				Type type = Type.GetType(orderedTypes[i]);
				if (type == null){
					Debug.LogError($"TypeSorter initialization error! {orderedTypes[i]} is not the name of an extant type! ");
				} else {
					priority.Add(type, i-halfLength);
				}
			}
		}
		
		// Compares the priority int of the Types of the TypedCallbacks.
		// Doesn't return zero if it's the same type (with same priority), since 0 is equality and
		// equality is only when the callback actions are exactly the same.
		public int Compare(TypedCallback left, TypedCallback right){
			if (left.Value == right.Value){
				return 0;
			}
			int leftPriority = 0, rightPriority = 0;
			// Default int value of 0 when the dictionary doesn't have a key is desired behavior.
			if (left.Type != null){
				priority.TryGetValue(left.Type, out leftPriority);
			}
			if (right.Type != null){
				priority.TryGetValue(right.Type, out rightPriority);
			}
			int result = leftPriority-rightPriority;
			return result == 0 ? 1 : result;
		}
	}
	internal struct TypedCallback {
		public Type Type;
		public Action Value;
	}
}