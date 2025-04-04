using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Military {
	public abstract class Location<T> where T : Branch {
		private readonly List<Unit<T>> units = new();
		
		public abstract string Name {get;}
		public virtual Province SearchTargetProvince => Province;
		public abstract Province Province {get;}
		public abstract Vector3 WorldPosition {get;}

		public IEnumerable<Unit<T>> Units => units;
		
		public void Add(Unit<T> unit){
			units.Add(unit);
		}
		public void Remove(Unit<T> unit){
			units.Remove(unit);
		}
		
		public virtual void AdjustPath(List<ProvinceLink> path){}
		public override string ToString(){
			return Name;
		}
	}
}
