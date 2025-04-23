namespace Simulation.Military {
	public class Transport : Ship {
		public int ManpowerCapacity {get; private set;}
		
		protected override bool ShouldAvoidCombat => true;
		
		internal void Init(float attackPower, int hull, float size, float gold, int sailors, int manpowerCapacity){
			Init(attackPower, hull, size, gold, sailors);
			ManpowerCapacity = manpowerCapacity;
		}
	}
}
