using Simulation;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "TargetIsSafe", menuName = "ScriptableObjects/AI/Nodes/TargetIsSafe")]
	public class TargetIsSafe : TargetDecorator {
		protected override bool IsTargetValid(Province targetProvince){
			return !Brain.Controller.ShouldAvoidArmyAt(targetProvince, Brain.Unit);
		}
	}
}
