namespace Simulation.Military {
	public class Army : Branch {
		private readonly Unit<Army> unit;
		
		public Army(Unit<Army> armyUnit){
			unit = armyUnit;
		}
		public override bool LinkEvaluator(ProvinceLink link){
			return link is LandLink && (unit.Owner == link.Target.Owner || unit.Owner.GetDiplomaticStatus(link.Target.Owner).IsAtWar);
		}
		public override string CreatingVerb => "Recruiting";
	}
}
