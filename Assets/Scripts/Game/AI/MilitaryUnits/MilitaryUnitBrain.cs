namespace AI {
	public abstract class MilitaryUnitBrain<TUnit> : BehaviourTree.Brain where TUnit : Simulation.Military.Unit<TUnit> {
		private TickTree tickTree;
		
		public AIController Controller {get; private set;}
		public TUnit Unit {get; private set;}
		
		private void Awake(){
			// Enforce with [RequireComponent] in concrete implementations.
			Unit = GetComponent<TUnit>();
		}
		protected override void Start(){
			base.Start();
			Controller = Unit.Owner.GetComponent<AIController>();
			tickTree = (TickTree)Tree;
			tickTree.Init(Unit.Province.Calendar);
			
			if (Controller.enabled){
				OnEnable();
			} else {
				enabled = false;
			}
		}
		protected void OnEnable(){
			if (tickTree != null){
				tickTree.Enable();
			}
		}
		protected void OnDisable(){
			tickTree.Disable();
		}
		protected void OnDestroy(){
			Destroy(tickTree);
		}
	}
}
