using System.Collections;
using UnityEngine;

namespace BehaviourTree.Nodes {
	public class Compare : DecoratorNode {
		public enum Operator {
			Equals,
			NotEquals,
			GreaterThan,
			LessThan
		};

		public string key = "VariableName"; 
		public Operator @operator = Operator.LessThan;
		public int compareValue = 5;
		public bool doCompareEveryFrame = true;
		private bool resultFlag;
		private static readonly string[] OperatorCodes = {" == ", " != ", " > ", " < "};

		#region Properties
		public override string Description => key+OperatorCodes[(int)@operator]+compareValue+(doCompareEveryFrame ? " (EF)" : "");
		#endregion

		protected void UpdateResult(){
			resultFlag = false;
			int integerValue = 0;
			if (Tree != null &&
			    Tree.Blackboard != null &&
			    Tree.Blackboard.TryGetValue(key, out object value)){
				switch(value){
					case int integer:
						integerValue = integer;
						break;
					case bool boolean:
						integerValue = boolean ? 1 : 0;
						break;
					case float floatValue:
						integerValue = Mathf.RoundToInt(floatValue);
						break;
					case double doubleValue:
						integerValue = Mathf.RoundToInt((float)doubleValue);
						break;
					case IEnumerable enumerable: {
						IEnumerator e = enumerable.GetEnumerator();
						while (e.MoveNext()){
							integerValue++;
						}
						break;
					}
				}
			}
			switch(@operator){
				case Operator.Equals:
					resultFlag = integerValue == compareValue;
					break;
				case Operator.NotEquals:
					resultFlag = integerValue != compareValue;
					break;
				case Operator.GreaterThan:
					resultFlag = integerValue > compareValue;
					break;
				case Operator.LessThan:
					resultFlag = integerValue < compareValue;
					break;
			}
		}
		protected override void OnStart(){
			base.OnStart();
			if (!doCompareEveryFrame){
				UpdateResult();
			}
		}
		protected override State OnUpdate(){
			if (doCompareEveryFrame){
				UpdateResult();
			}
			if (!resultFlag){
				CurrentState = State.Failure;
			} else if (child != null){
				CurrentState = child.Update();
			}
			return CurrentState;
		}
	}
}
