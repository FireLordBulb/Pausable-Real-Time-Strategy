namespace Simulation.Military {
	public enum MoveOrderResult {
		Success,
		AlreadyAtDestination,
		BusyRetreating,
		NotBuilt,
		NoPath,
		NoAccess,
		InvalidTarget,
		NotOwner,
		NotDestinationOwner
	}
}