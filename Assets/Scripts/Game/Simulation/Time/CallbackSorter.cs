using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation {
	internal class CallbackSorter : IComparer<Action> {
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
		
		public int Compare(Action left, Action right){
			if (left == null){
				return right == null ? 0 : +1;
			}
			if (right == null){
				return -1;
			}
			if (left.Equals(right)){
				return 0;
			}
			if (DoesNotContainKey(left)){
				return +1;
			}
			if (DoesNotContainKey(right)){
				return -1;
			}
			int result = GetPriority(left)-GetPriority(right);
			// Equal priority isn't enough to count as equality so the result isn't allowed to be 0.
			if (result == 0){
				return HashCode.Combine(left.Target, left.Method.MethodHandle.Value)-HashCode.Combine(right.Target, right.Method.MethodHandle.Value);
			}
			return result;
		}
		private bool DoesNotContainKey(Action action){
			Type type = action.Target?.GetType();
			if (type == null){
				Debug.LogWarning($"Action with static function: {action.Method}");
				return false;
			}
			bool doesNotContain = !priority.ContainsKey(type);
			if (doesNotContain){
				Debug.LogWarning($"Action with target of type not in sort order: {type}");
			}
			return doesNotContain;
		}
		private int GetPriority(Action action) => priority[action.Target.GetType()];
	}
}
