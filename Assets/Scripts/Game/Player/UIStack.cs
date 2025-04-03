using System.Collections.Generic;
using ActionStackSystem;
using Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Player {
	[RequireComponent(typeof(Canvas))]
	public class UIStack : ActionStack<UILayer> {
		private const float ActivationThreshold = 0.5f;

		public static UIStack Instance {get; private set;}
		
		[SerializeField] private HUD hud;
		[SerializeField] private UILayer countrySelection;
		[SerializeField] private UILayer provinceWindow;
		[SerializeField] private UILayer countryWindow;
		[SerializeField] private UILayer regimentWindow;
		[SerializeField] private DebugConsole debugConsole;
		[SerializeField] private Button closeButton;
		[SerializeField] private LayerMask mapClickMask;
		[SerializeField] private int maxSelectHistory;
		
		private Input.UIActions input;
		private UILayer layerToPush;
		private readonly LinkedList<Component> selectedHistory = new();
		private int selectHistoryCount;
		private Component hoveredSelectable;
		private Component mouseDownSelectable;
		private bool isSelectClickRight;
		
		public bool CanSwitchCountry {get; internal set;}
		public Country PlayerCountry {get; private set;}
		public Component Selected {get; private set;}
		
		public CalendarPanel CalendarPanel => hud.CalendarPanel;
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
			input.Click.performed      += context => OnClick(context, false);
			input.RightClick.performed += context => OnClick(context, true );
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

		private void OnClick(InputAction.CallbackContext context, bool isRightClick){
			bool isMouseDown = ActivationThreshold < context.ReadValue<float>();
			if (!hoveredSelectable){
				return;
			}
			if (isMouseDown){
				mouseDownSelectable = hoveredSelectable;
				isSelectClickRight = isRightClick;
				return;
			}
			// Ignore mouse up for the mouse button pressed down less recently, if both mouse buttons, were down.
			if (isSelectClickRight != isRightClick){
				return;
			}
			if (mouseDownSelectable == hoveredSelectable){
				Select(CurrentAction.OnSelectableClicked(mouseDownSelectable, isRightClick));
			} else {
				// Dragging a left click always results in a deselect, no layer-specific logic for this.
				Deselect();
			}
			mouseDownSelectable = null;
		}
		
		public void DelayedPush(UILayer layer) {
			layerToPush = layer;
		}
		// If the pushed layer is a prefab (thus not part of a valid scene), instantiate it before actually pushing it.
		public override void Push(UILayer layer){
			if (!layer.gameObject.scene.IsValid()){
				layer = Instantiate(layer, transform);
				if (layer is IClosableWindow closableWindow){
					Button button = Instantiate(closeButton, layer.transform);
					button.onClick.AddListener(closableWindow.Close);
				}
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
				// TODO: hovering UI elements.
				if (hit.collider.TryGetComponent(out RegimentClickCollider regimentCollider)){
					hoveredSelectable = regimentCollider.Regiment;
				}
				return;
			}
			if (province == hoveredSelectable){
				return;
			}
			EndHover();
			province.OnHoverEnter();
			hoveredSelectable = province;
		}
		private bool MouseRaycast(out RaycastHit hit){
			if (EventSystem.current.IsPointerOverGameObject()){
				hit = new RaycastHit();
				return false;
			}
			return Physics.Raycast(CameraMovement.Instance.Camera.ScreenPointToRay(MousePosition), out hit, float.MaxValue, mapClickMask);
		}
		private void EndHover(){
			if (!hoveredSelectable){
				return;
			}
			if (hoveredSelectable is Province province){
				province.OnHoverLeave();
			}
			hoveredSelectable = null;
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

		public void RefreshSelected(){
			if (Selected is Province){
				RefreshWindow<ProvinceWindow>();
			} else if (Selected is Country){
				RefreshWindow<CountryWindow>();
			} else if (Selected is Simulation.Military.Regiment){
				RefreshWindow<RegimentWindow>();
			}
		}
		private void RefreshWindow<T>() where T : IRefreshable {
			for (int i = StackList.Count-1; i > 0; i--){
				if (StackList[i] is T window){
					window.Refresh();
					break;
				}
			}
		}
		
		public void Select(Component component){
			SelectWithoutHistoryUpdate(component);
			UpdateSelectedHistory();
		}
		private void SelectWithoutHistoryUpdate(Component selectable){
			if (Selected == selectable){
				return;
			}
			Selected = selectable;
			// Delay the push until after the next OnUpdate() so the current window can remove itself first.
			if (Selected is Province){
				DelayedPush(provinceWindow);
			} else if (Selected is Country){
				DelayedPush(countryWindow);
			} else if (Selected is Simulation.Military.Regiment){
				DelayedPush(regimentWindow);
			}
		}
		public void Deselect(Component component){
			if (Selected == component){
				Deselect();
			}
		}
		public void Deselect(){
			Selected = null;
			UpdateSelectedHistory();
		}

		private void UpdateSelectedHistory(){
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

		// Different from CurrentAction since that might be null right after a new layer has been pushed.
		public UILayer GetTopLayer(){
			return StackList[^1];
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