using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Military {
	public class TransportDeck : Location<Regiment> {
		public readonly Transport Transport;

		public override string Name => $"{Transport.Type.name} at {Transport.Location.Name}";
		public override Province Province => Transport.Province;
		public override Vector3 WorldPosition => Transport.transform.position;
		public int CurrentManpower => Units.Sum(unit => unit.CurrentManpower);
		
		public TransportDeck(Transport transport){
			Transport = transport;
		}
		
		// Battles should never be able to happen on transports.
		protected override bool AreHostile(Country defender, Country attacker){
			return false;
		}
		internal override void Refresh(){
			Transport.Location.Refresh();
		}
		
		public override void AdjustPathStart(List<ProvinceLink> path){
			path.Insert(0, ((Harbor)Transport.Location).Coast);
		}
		public override void AdjustPathEnd(List<ProvinceLink> path){
			path.Add(((Harbor)Transport.Location).Coast.Reverse);
		}
	}
}
