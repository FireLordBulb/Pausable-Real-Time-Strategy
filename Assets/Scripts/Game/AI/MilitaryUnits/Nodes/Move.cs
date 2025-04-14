using System.Collections.Generic;
using System.Linq;
using Simulation;
using Simulation.Military;
using UnityEngine;

namespace AI.Nodes {
	[CreateAssetMenu(fileName = "MoveRegiment", menuName = "ScriptableObjects/AI/Nodes/MoveRegiment")]
	public class Move : MilitaryUnitNode<Regiment> {
		protected override void OnStart(){
			base.OnStart();
			IEnumerable<ProvinceLink> links = Brain.Unit.Location.Province.Links;
			Province target = links.ElementAt(Random.Range(0, links.Count())).Target;
			Brain.Controller.Country.MoveRegimentTo(Brain.Unit, target);
		}
		protected override void OnStop(){
			
		}
		protected override State OnUpdate(){
			return Brain.Unit.IsMoving ? State.Running : State.Success;
		}
	}
}