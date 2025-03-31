using System.Collections.Generic;
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
		
		[SerializeField] private HUD hud;
		[SerializeField] private UILayer countrySelection;
		[SerializeField] private UILayer provinceWindow;
		[SerializeField] private UILayer countryWindow;
		[SerializeField] private DebugConsole debugConsole;
		[SerializeField] private LayerMask mapClickMask;
		[SerializeField] private int maxSelectHistory;

		private Input.UIActions input;
		private UILayer layerToPush;
		private readonly LinkedList<Component> selectedHistory = new();
		private int selectHistoryCount;
		private Province hoveredProvince;
		private Province mouseDownProvince;
		private bool isProvinceClickRight;
		
		public bool CanSwitchCountry {get; internal set;}
		public Country PlayerCountry {get; private set;}
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
			input.Back.canceled += _ => {
				if (selectHistoryCount == 0){
					return;
				}
				selectedHistory.RemoveFirst();
				selectHistoryCount--;
				SelectWithoutHistoryUpdate(selectHistoryCount != 0 ? selectedHistory.First.Value : null);
			};
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			debugConsole = Instantiate(debugConsole, transform);
			bool debugWasDeactivated = false;
			input.Debug.canceled += _ => {
				if (debugConsole.gameObject.activeSelf){
					return;
				}
				if (debugWasDeactivated){
					debugWasDeactivated = false;
					return;
				}
				debugConsole.Enable();
			};
			input.Debug.performed += _ => {
				if (!debugConsole.gameObject.activeSelf){
					return;
				}
				debugWasDeactivated = true;
				debugConsole.Disable();
			};
#endif
			
			hud = Instantiate(hud, transform);
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
				Select (CurrentAction.OnProvinceClicked(mouseDownProvince, isRightClick));
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

		public void PlayAs(Country country){
			PlayerCountry = country;
			hud.RefreshCountry();
			if (country == null){
				return;
			}
			CameraMovement cameraMovement = CameraMovement.Instance;
			cameraMovement.SetZoom(cameraMovement.MaxZoom, cameraMovement.Camera.WorldToScreenPoint(country.Capital.WorldPosition));
		}
		
		public void Select(Component component){
			SelectWithoutHistoryUpdate(component);
			UpdateSelectedHistory(Selected);
		}
		private void SelectWithoutHistoryUpdate(Component component){
			Selected = component;
			// Delay the push until after the next OnUpdate() so the current window can remove itself first.
			if (Selected is Province){
				DelayedPush(provinceWindow);
			} else if (Selected is Country){
				DelayedPush(countryWindow);
			}
		}
		public void Deselect(Component component){
			if (Selected == component){
				Deselect();
			}
		}
		public void Deselect(){
			Selected = null;
			UpdateSelectedHistory(Selected);
		}

		private void UpdateSelectedHistory(Component component){
			if (selectHistoryCount != 0 && selectedHistory.First.Value == Selected){
				return;
			}
			selectedHistory.AddFirst(Selected);
			selectHistoryCount++;
			while (maxSelectHistory < selectHistoryCount){
				selectedHistory.RemoveLast();
				selectHistoryCount--;
			}
		}
		public void ClearSelectHistory(){
			selectedHistory.Clear();
			selectHistoryCount = 0;
		}

		public UILayer GetLayerBelow(UILayer layer){
			StackList.RemoveAll(l => l == null);
			int index = StackList.FindIndex(l => l == layer);
			if (index < 1){
				return null;
			}
			return StackList[index-1];
		}
	}
}