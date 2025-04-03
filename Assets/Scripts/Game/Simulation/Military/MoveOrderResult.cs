namespace Simulation.Military {
	public enum MoveOrderResult {
		Success,
		AlreadyAtDestination,
		NotBuilt,
		NoPath,
		NoAccess,
		InvalidTarget,
		NotOwner
	}
}