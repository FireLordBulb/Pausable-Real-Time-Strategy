using ActionStackSystem;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIStack : ActionStack<UILayer> {
	// If the pushed layer is a prefab (thus not part of a valid scene), instantiate it before actually pushing it.
	public override void Push(UILayer layer){
		if (!layer.gameObject.scene.IsValid()){
			layer = Instantiate(layer, transform);
		}
		base.Push(layer);
	}
}
