using System.Collections.Generic;
using UnityEngine;

namespace ActionStackSystem {
	public class ActionStack : ActionStack<IAction> {}
	
	public class ActionStack<T> : MonoBehaviour, IReadOnlyActionStack<T> where T : class, IAction {
		private readonly List<T> stackList = new();
		private readonly HashSet<T> startedActions = new();
		private T currentAction;

		#region Properties
		
		public T CurrentAction => currentAction;
		public IEnumerable<T> Actions => stackList;

		#endregion

		public virtual void Clear(){
			stackList.Clear();
			currentAction = null;
		}
		public virtual void Push(T action){
			if (action == null || action == currentAction){
				return;
			}
			// Remove if already on stack, duplicates forbidden.
			stackList.RemoveAll(a => a == action);
			
			stackList.Add(action);
			currentAction = null;
		}
		public virtual void Remove(T action){
			if (action == null || !stackList.Contains(action)){
				return;
			}
			if (action == currentAction || startedActions.Contains(action)){
				action.OnEnd();
				currentAction = null;
			}
			stackList.Remove(action);
		}
		protected virtual void Update(){
			UpdateActions();
		}
		protected virtual void UpdateActions(){
			if (stackList.Count == 0){
				return;
			}
			// Pick a new current action if there is none
			if (currentAction == null){
				startedActions.RemoveWhere(action => action == null);
				currentAction = stackList[^1];
				bool bFirstTime = !startedActions.Contains(currentAction);
				startedActions.Add(currentAction);
				currentAction.OnBegin(bFirstTime);

				// Did the action change the stack in OnBegin()?
				if (stackList.Count > 0 && currentAction != stackList[^1]){
					currentAction = null;
					UpdateActions();
				}
			}

			if (currentAction == null){
				return;
			}
			currentAction.OnUpdate();
			
			if (stackList.Count == 0 || currentAction != stackList[^1] || !currentAction.IsDone()){
				return;
			}
			stackList.RemoveAt(stackList.Count-1);
			startedActions.Remove(currentAction);
			currentAction.OnEnd();
			currentAction = null;
		}
#if UNITY_EDITOR
		protected virtual void OnGUI(){
			if (gameObject.name != "UI"){
				return;
			}
			const float lineHeight = 32.0f;
			GUI.color = new Color(0.0f, 0.0f, 0.0f, 0.7f);
			Rect r = new(0, 0, 250.0f, lineHeight*stackList.Count);
			GUI.DrawTexture(r, Texture2D.whiteTexture);
			Rect line = new(10, 0, r.width-20, lineHeight);
			for (int i = 0; i < stackList.Count; i++){
				GUI.color = stackList[i] == currentAction ? Color.green : Color.white;
				GUI.Label(line, "#"+i+": "+stackList[i],
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