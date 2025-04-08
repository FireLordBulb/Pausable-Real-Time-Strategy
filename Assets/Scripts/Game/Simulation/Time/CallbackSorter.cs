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
			// Types not in the priority Dictionary get lower priority than all Types in it.
			if (left.Type == null || !priority.ContainsKey(left.Type) || right.Type == null || !priority.ContainsKey(right.Type)){
				return 1;
			}
			int result = priority[left.Type]-priority[right.Type];
			return result == 0 ? 1 : result;
		}
	}
	internal struct TypedCallback {
		public Type Type;
		public Action Value;
	}
}