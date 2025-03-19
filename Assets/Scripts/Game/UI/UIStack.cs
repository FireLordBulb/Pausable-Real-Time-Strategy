using ActionStackSystem;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIStack : ActionStack<UILayer> {
	private const float ActivationThreshold = 0.5f;

	[SerializeField] private LayerMask mapClickMask;
	public Camera Camera {get; private set;}

	public LayerMask MapClickMask => mapClickMask;
	
	private Input.UIActions input;
	private void Awake(){
		input = new Input().UI;
		input.Enable();
		input.Click.performed += context => {
			if (context.ReadValue<float>() < ActivationThreshold){
				CurrentAction.ReceiveClick(input.MousePosition.ReadValue<Vector2>());
			}
		};

		GameObject defaultLayer = new(){
			transform = {
				parent = transform
			}
		};
		Push(defaultLayer.AddComponent<UILayer>());
	}
	private void Start(){
		Camera = Camera.main;
	}

	// If the pushed layer is a prefab (thus not part of a valid scene), instantiate it before actually pushing it.
	public override void Push(UILayer layer){
		layer.Init(this);
		if (!layer.gameObject.scene.IsValid()){
			layer = Instantiate(layer, transform);
		}
		base.Push(layer);
	}
}
