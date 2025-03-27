using UnityEditor;

namespace Simulation {
	[CustomEditor(typeof(Country), true)]
	public class CountryEditor : MapGraphEditor {
		private void OnEnable(){
			Nodes = ((Country)target).Provinces;
		}
	}
}
