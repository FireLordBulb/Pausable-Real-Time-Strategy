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
				return right == null ? 0 : 1;
			}
			if (right == null){
				return -1;
			}
			if (left.Target == right.Target && left.Method == right.Method){
				return 0;
			}
			// Types not in the priority Dictionary get lower priority than all Types in it.
			if (ContainsKey(left) || ContainsKey(right)){
				return 1;
			}
			int result = GetPriority(left)-GetPriority(right);
			// Equal priority does not count as equality so result isn't allowed to be 0.
			return result == 0 ? 1 : result;
		}
		private bool ContainsKey(Action action) => action.Target == null || !priority.ContainsKey(action.Target.GetType());
		private int GetPriority(Action action) => priority[action.Target.GetType()];
	}
}
