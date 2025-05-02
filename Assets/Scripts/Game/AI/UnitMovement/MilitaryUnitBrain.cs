using System.Linq;
using Simulation;

namespace AI {
	public abstract class MilitaryUnitBrain<TUnit> : BehaviourTree.Brain where TUnit : Simulation.Military.Unit<TUnit> {
		private Calendar calendar;
		
		public AIController Controller {get; private set;}
		public TUnit Unit {get; private set;}
		
		// String names for blackboard stored here to remove need for string literals in specific node implementations.
		public string Target => "target";
		public string Harbor => "harbor";
		public string EnemyCountry => "enemyCountry";
		
		private void Awake(){
			base.Start();
			// Enforce with [RequireComponent] in concrete implementations.
			Unit = GetComponent<TUnit>();
		}
		protected override void Start(){
			Controller = Unit.Owner.GetComponent<AIController>();
			calendar = Unit.Province.Calendar;
			if (Controller.enabled){
				OnEnable();
			} else {
				enabled = false;
			}
		}
		protected override void Update(){
			// Do nothing in regular Update.
		}
		// Use the daily tick instead.
		private void DayTick(){
			if (this != null && enabled){
				base.Update();
			}
		}
		protected void OnEnable(){
			if (calendar != null){
				calendar.OnDayTick.AddListener(DayTick);
			}
		}
		protected void OnDisable(){
			if (calendar != null){
				calendar.OnDayTick.RemoveListener(DayTick);
			}
		}
		protected void OnDestroy(){
			Destroy(Tree);
		}

		internal bool IsReinforceableBattleOngoing(Simulation.Military.Location<TUnit> location){
			return location.IsBattleOngoing && location.Units.Any(regiment => regiment.Owner == Unit.Owner && !regiment.IsRetreating);
		}
	}
}
