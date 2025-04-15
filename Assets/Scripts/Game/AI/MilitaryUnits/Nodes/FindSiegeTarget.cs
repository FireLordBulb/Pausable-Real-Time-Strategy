using System.Collections.Generic;
using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "FindSiegeTarget", menuName = "ScriptableObjects/AI/Nodes/FindSiegeTarget")]
	public class FindSiegeTarget : MilitaryUnitNode<Regiment> {
		protected override void OnStart(){
			base.OnStart();
			
			IEnumerable<ProvinceLink> links = Brain.Unit.Location.Province.Links;
			Province target = links.ElementAt(Random.Range(0, links.Count())).Target;
			Tree.Blackboard.SetValue(Brain.Target, target);
			CurrentState = State.Success;
		}
		protected override State OnUpdate(){
			return CurrentState;
		}
		protected override void OnStop(){}
	}
}
