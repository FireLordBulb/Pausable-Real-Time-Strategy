using UnityEngine;

namespace Simulation.Military {
	public class SeaLocation : Location<Ship> {
		public readonly Sea Sea;
		
		public override string Name => Sea.Province.Name;
		public override Province Province => Sea.Province;
		public override Vector3 WorldPosition => Sea.transform.position;
		
		public SeaLocation(Sea sea){
			Sea = sea;
		}
		
		// Navies only fight if their countries are officially at war.
		protected override bool AreHostile(Country defender, Country attacker){
			return defender.GetDiplomaticStatus(attacker).IsAtWar;
		}
	}
}
