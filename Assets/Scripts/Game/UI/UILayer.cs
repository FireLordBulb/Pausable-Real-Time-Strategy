using ActionStackSystem;
using UnityEngine;

public class UILayer : ActionBehaviour {
	protected UIStack Stack;

	public void Init(UIStack uiStack){
		Stack = uiStack;
	}
	
	public override bool IsDone(){
		return false;
	}
	
	public void ReceiveClick(Vector2 mousePosition){
		if (!Physics.Raycast(Stack.Camera.ScreenPointToRay(mousePosition), out RaycastHit hit, float.MaxValue, Stack.MapClickMask)){
			return;
		}
		if (hit.collider.TryGetComponent(out Province province)){
			province.OnClick();
		}
	}
}
