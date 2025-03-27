using UnityEditor;

namespace Simulation {
	[CustomEditor(typeof(MapGraph), true)]
	public class MapEditor : MapGraphEditor {
		private void OnEnable(){
			Nodes = ((MapGraph)target).Nodes;
		}
	}
}
