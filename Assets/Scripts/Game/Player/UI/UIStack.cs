using ActionStackSystem;
using Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Player {
	[RequireComponent(typeof(Canvas))]
	public class UIStack : ActionStack<UILayer> {
		private const float ActivationThreshold = 0.5f;

		public static UIStack Instance {get; private set;}
		
		[SerializeField] private UILayer hud;
		[SerializeField] private UILayer countrySelection;
		[SerializeField] private UILayer provinceWindow;
		[SerializeField] private UILayer countryWindow;
		[SerializeField] private LayerMask mapClickMask;

		private Input.UIActions input;
		private UILayer layerToPush;
		private Province hoveredProvince;
		private Province mouseDownProvince;
		private bool isProvinceClickRight;

		public Country PlayerCountry {get; internal set;}
		public Component Selected {get; private set;}
		
		public Province SelectedProvince => Selected as Province;
		public Country SelectedCountry => Selected as Country;
		private Vector2 MousePosition => input.MousePosition.ReadValue<Vector2>();
		
		private void Awake(){
			if (Instance != null){
				Destroy(gameObject);
				return;
			}
			Instance = this;
			input = new Input().UI;
			input.Enable();
			input.Click.performed      += context => ClickProvince(context, false);
			input.RightClick.performed += context => ClickProvince(context, true );
			
			Push(hud);
			Push(countrySelection);
		}

		private void ClickProvince(InputAction.CallbackContext context, bool isRightClick){
			bool isMouseDown = ActivationThreshold < context.ReadValue<float>();
			if (!hoveredProvince){
				return;
			}
			if (isMouseDown){
				mouseDownProvince = hoveredProvince;
				isProvinceClickRight = isRightClick;
				return;
			}
			// Ignore mouse up for the mouse button pressed down less recently, if both mouse buttons, were down.
			if (isProvinceClickRight != isRightClick){
				return;
			}
			if (mouseDownProvince == hoveredProvince){
				Selected = CurrentAction.OnProvinceClicked(mouseDownProvince, isRightClick);
				// Delay the push until after the next OnUpdate() so the current window can remove itself first.
				if (Selected is Province){
					DelayedPush(provinceWindow);
				} else if (Selected is Country){
					DelayedPush(countryWindow);
				}
			} else {
				// Dragging a left click always results in a deselect, no layer-specific logic for this.
				Deselect();
			}
			mouseDownProvince = null;
		}
		
		public void DelayedPush(UILayer layer) {
			layerToPush = layer;
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
			if (layerToPush == null){
				return;
			}
			Push(layerToPush);
			layerToPush = null;
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
			return Physics.Raycast(CameraMovement.Instance.Camera.ScreenPointToRay(MousePosition), out hit, float.MaxValue, mapClickMask);
		}
		private void EndHover(){
			if (!hoveredProvince){
				return;
			}
			hoveredProvince.OnHoverLeave();
			hoveredProvince = null;
		}

		public void Deselect(Component province){
			if (Selected == province){
				Selected = null;
			}
		}
		public void Deselect(){
			Selected = null;
		}
	}
}