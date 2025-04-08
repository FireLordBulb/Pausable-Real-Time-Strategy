using Simulation;
using InterfaceSerialization;
using UnityEngine;

namespace Player {
	public class SelectableClickCollider : MonoBehaviour {
		[SerializeField, RequireInterface(typeof(ISelectable))] private MonoBehaviour selectable;
		public ISelectable Selectable => (ISelectable)selectable;
	}
}