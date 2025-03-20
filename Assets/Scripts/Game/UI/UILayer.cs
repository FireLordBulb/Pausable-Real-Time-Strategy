using ActionStackSystem;
using UnityEngine;

public class UILayer : ActionBehaviour {
	private Province savedProvince;
	
	public override bool IsDone(){
		return false;
	}
	
	public virtual void ReceiveClick(Vector2 mousePosition, bool isMouseDown){
		
	}
}
