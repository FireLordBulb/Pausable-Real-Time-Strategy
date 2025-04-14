using UnityEngine;

namespace BehaviourTree.Nodes {
	public abstract class Node : ScriptableObject {
		public enum State {
			Running,
			Success,
			Failure
		}

		[SerializeField] [HideInInspector] private int id;
		[SerializeField] [HideInInspector] public Vector2 position;
		[System.NonSerialized] public State CurrentState = State.Running;
		[System.NonSerialized] public bool IsStarted;
		private Tree tree;

		#region Properties
		public int ID => id;
		public Tree Tree => tree;
		public virtual string Description => "";
		#endregion

		private void OnValidate(){
			if (id == 0){
				id = System.Guid.NewGuid().GetHashCode();
			}
		}
		public State Update(){
			if (!IsStarted){
				OnStart();
				IsStarted = true;
			}
			CurrentState = OnUpdate();
			if (CurrentState != State.Running){
				OnStop();
				IsStarted = false;
			}
			return CurrentState;
		}
		public virtual void OnTreeStart(Tree tree){
			this.tree = tree;
		}
		protected abstract void OnStart();
		protected abstract void OnStop();
		protected abstract State OnUpdate();
		public virtual Node Clone(){
			Node clone = Instantiate(this);
			clone.name = name;
			return clone;
		}
	}
}
