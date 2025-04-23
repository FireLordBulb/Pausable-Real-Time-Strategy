namespace Simulation.Military {
	public class Transport : Ship {
		public TransportDeck Deck {get; private set;}
		public int ManpowerCapacity {get; private set;}
		
		protected override bool ShouldAvoidCombat => true;
		
		internal void Init(float attackPower, int hull, float size, float gold, int sailors, int manpowerCapacity){
			Init(attackPower, hull, size, gold, sailors);
			Deck = new TransportDeck(this);
			ManpowerCapacity = manpowerCapacity;
		}
		protected override void OnWorldPositionChanged(){
			foreach (Regiment regiment in Deck.Units){
				regiment.transform.position = transform.position;
			}
		}
	}
}
