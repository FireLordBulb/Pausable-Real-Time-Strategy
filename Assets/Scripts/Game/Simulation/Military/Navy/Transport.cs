using System.Linq;

namespace Simulation.Military {
	public class Transport : Ship {
		public TransportDeck Deck {get; private set;}
		public int ManpowerCapacity {get; private set;}
		
		protected override bool ShouldAvoidCombat => true;
		
		private void Awake(){
			Deck = new TransportDeck(this);
		}
		internal void Init(int manpowerCapacity){
			ManpowerCapacity = manpowerCapacity;
		}
		
		protected override void OnWorldPositionChanged(){
			foreach (Regiment regiment in Deck.Units){
				regiment.transform.position = transform.position;
			}
		}
		
		public bool CanRegimentBoard(Regiment regiment){
			return IsBuilt && regiment.Owner == Owner && !Location.IsBattleOngoing && regiment.CurrentManpower <= ManpowerCapacity-Deck.Units.Sum(unit => unit.CurrentManpower);
		}
		internal override void StackWipe(){
			foreach (Regiment regiment in Deck.Units.ToArray()){
				regiment.StackWipe();	
			}
			base.StackWipe();
		}
	}
}
