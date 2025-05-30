using System.Linq;
using UnityEngine;

namespace Simulation.Military {
	public class Transport : Ship {
		[SerializeField] private Transform regimentTransform;
		
		public TransportDeck Deck {get; private set;}
		public int ManpowerCapacity {get; private set;}
		
		public string UsedManpowerCapacityText => $"{Deck.CurrentManpower}/{ManpowerCapacity}";
		protected override bool ShouldAvoidCombat => true;
		
		private void Awake(){
			Deck = new TransportDeck(this);
		}
		internal void Init(int manpowerCapacity){
			ManpowerCapacity = manpowerCapacity;
		}
		
		protected override void OnWorldPositionChanged(){
			foreach (Regiment regiment in Deck.Units){
				regiment.transform.position = regimentTransform.position;
			}
		}
		
		public bool CanRegimentBoard(Regiment regiment){
			// Have to check if the transport has been destroyed by using Unity's overridden null equality.
			return this != null && IsBuilt && regiment.Owner == Owner && !Location.IsBattleOngoing && regiment.CurrentManpower <= ManpowerCapacity-Deck.CurrentManpower;
		}
		internal override void StackWipe(){
			foreach (Regiment regiment in Deck.Units.ToArray()){
				regiment.StackWipe();	
			}
			base.StackWipe();
		}
	}
}
