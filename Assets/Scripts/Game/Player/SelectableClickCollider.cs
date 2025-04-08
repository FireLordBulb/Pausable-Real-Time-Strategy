using Simulation;
using UnityEngine;
namespace Player {
	public class SelectableClickCollider : MonoBehaviour {
		[SerializeField
#if UNITY_EDITOR
		 , InterfaceSerialization.RequireInterface(typeof(ISelectable))
#endif
		] private MonoBehaviour selectable;
		public ISelectable Selectable => (ISelectable)selectable;
	}
}