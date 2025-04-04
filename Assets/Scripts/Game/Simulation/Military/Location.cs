using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Location<T> where T : Branch {
		public readonly List<Unit<T>> Units = new();
		
		public abstract string Name {get;}
		public virtual Province SearchTargetProvince => Province;
		public abstract Province Province {get;}
		public abstract Vector3 WorldPosition {get;}
		
		public virtual void AdjustPath(List<ProvinceLink> path){}
		public override string ToString(){
			return Name;
		}
	}
}
