using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public class TransportDeck : Location<Regiment> {
		public readonly Transport Transport;

		public override string Name => $"{Transport.Type.name} at {Transport.Location.Name}";
		public override Province Province => Transport.Province;
		public override Vector3 WorldPosition => Transport.transform.position;
		
		public TransportDeck(Transport transport){
			Transport = transport;
		}
		
		public override void AdjustPathStart(List<ProvinceLink> path){
			path.Insert(0, ((Harbor)Transport.Location).Coast);
		}
		public override void AdjustPathEnd(List<ProvinceLink> path){
			path.Add(((Harbor)Transport.Location).Coast.Reverse);
		}
	}
}
