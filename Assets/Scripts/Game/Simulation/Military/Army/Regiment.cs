using UnityEngine;

namespace Simulation.Military {
	public class Regiment : Unit<Regiment> {
		protected override Location<Regiment> GetLocation(ProvinceLink link){
			return link.Target.Land.ArmyLocation;
		}
		protected override (Location<Regiment>, int) CalculatePathLocation(){
			ProvinceLink link = PathToTarget[PathIndex];
			float terrainSpeedMultiplier = 1+0.5f*(link.Source.Terrain.MoveSpeedModifier+link.Target.Terrain.MoveSpeedModifier);
			return (GetLocation(link), Mathf.CeilToInt(link.Distance/(MovementSpeed*terrainSpeedMultiplier)));
		}
		
		public override void StackWipe(){
			Owner.RemoveRegiment(this);
			Destroy(gameObject);
		}
		
		public override bool LinkEvaluator(ProvinceLink link){
			return link is LandLink && (Owner == link.Target.Owner || Owner.GetDiplomaticStatus(link.Target.Owner).IsAtWar);
		}
		public override string CreatingVerb => "Recruiting";
	}
}