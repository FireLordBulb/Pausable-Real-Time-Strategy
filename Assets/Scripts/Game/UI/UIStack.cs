using ActionStackSystem;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
public class UIStack : ActionStack<UILayer> {
	private const float ActivationThreshold = 0.5f;

	public static UIStack Instance;
	
	[SerializeField] private UILayer hud;
	[SerializeField] private ProvinceWindow provinceWindow;
	[SerializeField] private LayerMask mapClickMask;

	private Input.UIActions input;
	private Province hoveredProvince;
	private Province mouseDownProvince;
	
	public Camera Camera {get; private set;}
	public Province SelectedProvince {get; private set;}
	
	public LayerMask MapClickMask => mapClickMask;
	public Vector2 MousePosition => input.MousePosition.ReadValue<Vector2>();
	
	private void Awake(){
		if (Instance != null){
			Destroy(gameObject);
			return;
		}
		Instance = this;
		input = new Input().UI;
		input.Enable();
		input.Click.performed += context => {
			bool isMouseDown = ActivationThreshold < context.ReadValue<float>();
			if (!hoveredProvince){
				return;
			}
			if (isMouseDown){
				mouseDownProvince = hoveredProvince;
				return;
			}
			if (SelectedProvince != null){
				SelectedProvince = null;
				return;
			}
			if (mouseDownProvince == hoveredProvince){
				SelectedProvince = mouseDownProvince;
				Push(provinceWindow);
			} else {
				SelectedProvince = null;
			}
			mouseDownProvince = null;
		};
		
		Push(hud);
	}
	private void Start(){
		Camera = Camera.main;
	}

	// If the pushed layer is a prefab (thus not part of a valid scene), instantiate it before actually pushing it.
	public override void Push(UILayer layer) {
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
		if (!MouseRaycast(out RaycastHit hit)){
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
		province.OnHoverEnter();
		hoveredProvince = province;
	}
	private bool MouseRaycast(out RaycastHit hit){
		if (EventSystem.current.IsPointerOverGameObject()){
			hit = new RaycastHit();
			return false;
		}
		return Physics.Raycast(Camera.ScreenPointToRay(MousePosition), out hit, float.MaxValue, MapClickMask);
	}
	private void EndHover(){
		if (!hoveredProvince){
			return;
		}
		hoveredProvince.OnHoverLeave();
		hoveredProvince = null;
	}
}
