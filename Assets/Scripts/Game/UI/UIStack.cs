using ActionStackSystem;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class UIStack : ActionStack<UILayer> {
	private const float ActivationThreshold = 0.5f;

	public static UIStack Instance;
	
	[SerializeField] private UILayer hud;
	[SerializeField] private LayerMask mapClickMask;
	public Camera Camera {get; private set;}

	public LayerMask MapClickMask => mapClickMask;
	public Vector2 MousePosition => input.MousePosition.ReadValue<Vector2>();
	
	private Input.UIActions input;
	private Province hoveredProvince;
	
	private void Awake(){
		if (Instance != null){
			Destroy(gameObject);
			return;
		}
		Instance = this;
		input = new Input().UI;
		input.Enable();
		input.Click.performed += context => {
			CurrentAction.ReceiveClick(MousePosition,  ActivationThreshold < context.ReadValue<float>());
		};
		
		Push(hud);
	}
	private void Start(){
		Camera = Camera.main;
	}

	// If the pushed layer is a prefab (thus not part of a valid scene), instantiate it before actually pushing it.
	public override void Push(UILayer layer){
		if (!layer.gameObject.scene.IsValid()){
			layer = Instantiate(layer, transform);
		}
		base.Push(layer);
	}
	protected override void Update(){
		UpdateHover();
		base.Update();
	}
	private void UpdateHover(){
		if (!Physics.Raycast(Camera.ScreenPointToRay(MousePosition), out RaycastHit hit, float.MaxValue, MapClickMask)){
			EndHover();
			return;
		}
		if (!hit.collider.TryGetComponent(out Province province)){
			EndHover();
			// TODO: hovering UI elements and army units.
			return;
		}
		if (province == hoveredProvince){
			return;
		}
		EndHover();
		province.OnHover();
		hoveredProvince = province;
	}
	private void EndHover(){
		if (!hoveredProvince){
			return;
		}
		hoveredProvince.OnHoverEnd();
		hoveredProvince = null;
	}
}
