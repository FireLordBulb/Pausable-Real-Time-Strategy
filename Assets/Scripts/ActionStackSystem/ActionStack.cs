using System.Collections.Generic;
using UnityEngine;

namespace ActionStackSystem {
	public class ActionStack : ActionStack<IAction> {}
	
	public class ActionStack<T> : MonoBehaviour, IReadOnlyActionStack<T> where T : class, IAction {
		private readonly HashSet<T> startedActions = new();
		private T currentAction;
		
		protected List<T> StackList {get;} = new();
		
		public T CurrentAction => currentAction;
		public IEnumerable<T> Actions => StackList;

		public virtual void Clear(){
			StackList.Clear();
			currentAction = null;
		}
		public virtual void Push(T action){
			if (action == null || action == currentAction){
				return;
			}
			// Remove if already on stack, duplicates forbidden.
			StackList.RemoveAll(a => a == action);
			
			StackList.Add(action);
			currentAction = null;
		}
		public virtual bool Remove(T action){
			if (action == null || !StackList.Contains(action)){
				return false;
			}
			if (action == currentAction || startedActions.Contains(action)){
				action.OnEnd();
				currentAction = null;
			}
			return StackList.Remove(action);
		}
		public void RemoveAndAbove(T action){
			int index = StackList.FindIndex(a => a == action);
			bool wasRemoved = StackList.Remove(action);
			if (wasRemoved){
				StackList.RemoveRange(index, StackList.Count-index);	
			}
		}
		protected virtual void Update(){
			UpdateActions();
		}
		protected virtual void UpdateActions(){
			if (StackList.Count == 0){
				return;
			}
			// Pick a new current action if there is none
			if (currentAction == null){
				startedActions.RemoveWhere(action => action == null);
				currentAction = StackList[^1];
				bool bFirstTime = !startedActions.Contains(currentAction);
				startedActions.Add(currentAction);
				currentAction.OnBegin(bFirstTime);

				// Did the action change the stack in OnBegin()?
				if (StackList.Count > 0 && currentAction != StackList[^1]){
					currentAction = null;
					UpdateActions();
				}
			}

			if (currentAction == null){
				return;
			}
			currentAction.OnUpdate();
			
			if (StackList.Count == 0 || currentAction != StackList[^1] || !currentAction.IsDone()){
				return;
			}
			StackList.RemoveAt(StackList.Count-1);
			startedActions.Remove(currentAction);
			currentAction.OnEnd();
			currentAction = null;
		}
#if UNITY_EDITOR
		protected virtual void OnGUI(){
			if (gameObject.name != "OnGUI is disabled!"){
				return;
			}
			const float lineHeight = 32.0f;
			GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
			Rect r = new(0, 0, 250.0f, lineHeight*StackList.Count);
			GUI.DrawTexture(r, Texture2D.whiteTexture);
			Rect line = new(10, 0, r.width-20, lineHeight);
			for (int i = 0; i < StackList.Count; i++){
				GUI.color = StackList[i] == currentAction ? Color.green : Color.white;
				GUI.Label(line, "#"+i+": "+StackList[i],
					i == 0 ? UnityEditor.EditorStyles.boldLabel : UnityEditor.EditorStyles.label);
				line.y += line.height;
			}
		}
#endif
	}

	public interface IReadOnlyActionStack<out T> where T : IAction {
		public T CurrentAction {get;}
		public IEnumerable<T> Actions {get;}
	}
	
	public interface IAction {
		void OnBegin(bool isFirstTime);
		void OnUpdate();
		void OnEnd();
		bool IsDone();
	}
	public abstract class ActionAdapter : IAction {
		public virtual void OnBegin(bool isFirstTime){}
		public virtual void OnUpdate(){}
		public virtual void OnEnd(){}
		public virtual bool IsDone(){
			return true;
		}
	}
	public abstract class ActionBehaviour : MonoBehaviour, IAction {
		public virtual void OnBegin(bool isFirstTime){}
		public virtual void OnUpdate(){}
		public virtual void OnEnd(){}
		public virtual bool IsDone(){
			return true;
		}
	}
}