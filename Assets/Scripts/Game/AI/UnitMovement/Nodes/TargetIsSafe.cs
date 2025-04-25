using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "TargetIsSafe", menuName = "ScriptableObjects/AI/Nodes/TargetIsSafe")]
	public class TargetIsSafe : TargetDecorator {
		protected override bool IsTargetValid(Location<Regiment> targetLocation){
			return !Brain.Controller.ShouldAvoidArmyAt(targetLocation.Province, Brain.Unit);
		}
	}
}
