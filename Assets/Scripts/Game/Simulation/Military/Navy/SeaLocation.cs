using System.Collections.Generic;
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
			return AreAtWar(defender, attacker);
		}
		internal static bool AreAtWar(Country defender, Country attacker){
			return defender.GetDiplomaticStatus(attacker).IsAtWar;
		}
		internal override void Refresh(){
			Refresh(this);
		}
		internal static void Refresh(Location<Ship> location){
			Dictionary<(Location<Ship>, BattleSide), int> regimentSharedIndices = new();
			location.Refresh((ship, key) => {
				if (ship is not Transport transport){
					return;
				}
				foreach (Regiment regiment in transport.Deck.Units){
					if (!regimentSharedIndices.TryGetValue(key, out int regimentIndex)){
						regimentIndex = 0;
					}
					regiment.SetSharedPositionIndex(regimentIndex);
					regimentSharedIndices[key] = regimentIndex+1;
				}
			});
		}
	}
}
