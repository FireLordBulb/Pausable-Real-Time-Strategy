using ActionStackSystem;
using UnityEngine;

public class UILayer : ActionBehaviour {
	protected UIStack Stack {get; private set;}

	public override void OnBegin(bool isFirstTime){
		base.OnBegin(isFirstTime);
		Stack = UIStack.Instance;
	}
	public override void OnEnd(){
		Destroy(gameObject);
	}
	public override bool IsDone(){
		return false;
	}
}
