using UnityEditor;

namespace Simulation {
	[CustomEditor(typeof(Province), true)]
	public class ProvinceEditor : MapGraphEditor {
		private void OnEnable(){
			Nodes = new[]{(Province)target};
		}
	}
}
