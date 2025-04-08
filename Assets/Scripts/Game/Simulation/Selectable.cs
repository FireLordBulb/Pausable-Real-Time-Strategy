namespace Simulation {
	// Exists so the Player UI can select stuff without making the Simulation assembly aware of the Player assembly. 
	public interface ISelectable {
		public void OnSelect();
		public void OnDeselect();
	}
}