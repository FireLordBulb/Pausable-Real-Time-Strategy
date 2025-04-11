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
		#region SerializedFields
		[Header("Scene Init Prefabs")]
		[SerializeField] private MapGraph map;
		[SerializeField] private CameraInput cameraInput;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		[SerializeField] private DebugConsole debugConsole;
#endif
		[Header("Layer Prefabs")]
		[SerializeField] private HUD hud;
		[SerializeField] private UILayer countrySelection;
		[SerializeField] private UILayer provinceWindow;
		[SerializeField] private UILayer countryWindow;
		[SerializeField] private UILayer regimentWindow;
		[SerializeField] private UILayer shipWindow;
		[Space]
		[SerializeField] private Button closeButton;
		[SerializeField] private LayerMask mapClickMask;
		[SerializeField] private int maxSelectHistory;
		#endregion
		#region Private Fields
		private Input.UIActions input;
		private CameraMovement cameraMovement;
		private Camera mainCamera;
		private Vector3 mouseWorldPosition;
		
		private readonly LinkedList<ISelectable> selectedHistory = new();
		private int selectHistoryCount;
		private ISelectable hoveredSelectable;
		private ISelectable mouseDownSelectable;
		private bool isSelectClickRight;
		#endregion
		#region Auto-Properties
		public bool CanSwitchCountry {get; internal set;}
		public Country PlayerCountry {get; private set;}
		public bool HasPlayerCountryChanged {get; private set;}
		public ISelectable Selected {get; private set;}
		public Links Links {get; private set;}
		public bool IsShiftHeld {get; private set;}
		public bool IsControlHeld {get; private set;}
		#endregion
		#region Getter Properties
		public MapGraph Map => map;
		public Province SelectedProvince => Selected as Province;
		public Country SelectedCountry => Selected as Country;
		private Vector3 MousePosition => input.MousePosition.ReadValue<Vector2>();
		#endregion

		#region Awake
		private void Awake(){
			InitScene();
			SpawnUI();
			EnableInput();
		}
		private void InitScene(){
			map = Instantiate(map);
			map.gameObject.name = "Map";
			cameraInput = Instantiate(cameraInput);
			cameraInput.gameObject.name = "MainCamera";
			cameraMovement = cameraInput.Movement;
			cameraMovement.Map = map;
			mainCamera = cameraMovement.Camera;
		}	
		private void SpawnUI(){
			Links = new Links(Select);
			hud = Instantiate(hud, transform);
			hud.CalendarPanel.Calendar = map.Calendar;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			debugConsole = Instantiate(debugConsole, transform);
			debugConsole.UI = this;
			debugConsole.Calendar = map.Calendar;
			debugConsole.CalendarPanel = hud.CalendarPanel;
			cameraInput.DebugConsole = debugConsole;
			hud.CalendarPanel.DebugConsole = debugConsole;
#endif
			Push(hud);
			Push(countrySelection);
			map.Calendar.OnMonthTick.AddListener(Refresh);
		}
		private void EnableInput(){
			input = new Input().UI;
			input.Enable();
			input.Click.performed      += context => OnClick(context, false);
			input.RightClick.performed += context => OnClick(context, true );
			input.Shift.performed += _ => {
				IsShiftHeld = true;
			};
			input.Shift.canceled += _ => {
				IsShiftHeld = false;
			};
			input.Control.performed += _ => {
				IsControlHeld = true;
			};
			input.Control.canceled += _ => {
				IsControlHeld = false;
			};
			input.Back.canceled += _ => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (debugConsole.IsKeyboardBusy){
					return;
				}
#endif
				if (selectHistoryCount == 0){
					return;
				}
				selectedHistory.RemoveFirst();
				selectHistoryCount--;
				SelectWithoutHistoryUpdate(selectHistoryCount != 0 ? selectedHistory.First.Value : null);
			};
			input.Cancel.canceled += _ => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				if (debugConsole.IsKeyboardBusy){
					return;
				}
#endif
				if (CurrentAction is IClosableWindow closableWindow){
					closableWindow.Close();
				}
			};
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
		}
		private void OnClick(InputAction.CallbackContext context, bool isRightClick){
			bool isMouseDown = ActivationThreshold < context.ReadValue<float>();
			if (hoveredSelectable == null){
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
			}
			mouseDownSelectable = null;
		}
		#endregion
		
		public TLayer Push<TLayer>(TLayer layer) where TLayer : UILayer {
			// If the pushed layer is a prefab (thus not part of a valid scene), instantiate it before actually pushing it.
			if (!layer.gameObject.scene.IsValid()){
				layer = Instantiate(layer, transform);
				if (layer is IClosableWindow closableWindow){
					Button button = Instantiate(closeButton, layer.transform);
					button.onClick.AddListener(closableWindow.Close);
				}
			}
			layer.Init(this);
			base.Push(layer);
			return layer;
		}
		
		#region Update
		protected override void Update(){
			UpdateHover();
			base.Update();
		}
		private void UpdateHover(){
			if (EventSystem.current.IsPointerOverGameObject()){
				EndHover();
				return;
			}
			if (!Physics.Raycast(mainCamera.ScreenPointToRay(MousePosition), out RaycastHit hit, float.MaxValue, mapClickMask)){
				EndHover();
				return;
			}
			mouseWorldPosition = hit.point;
			if (!hit.collider.TryGetComponent(out Province province)){
				EndHover();
				// TODO: hovering world-space UI elements.
				if (hit.collider.TryGetComponent(out SelectableClickCollider clickCollider)){
					hoveredSelectable = clickCollider.Selectable;
				}
				return;
			}
			if (ReferenceEquals(province, hoveredSelectable)){
				return;
			}
			EndHover();
			province.OnHoverEnter();
			hoveredSelectable = province;
		}
		private void EndHover(){
			if (hoveredSelectable == null){
				return;
			}
			if (hoveredSelectable is Province province){
				province.OnHoverLeave();
			}
			hoveredSelectable = null;
		}
		#endregion

		#region Public Void Methods
		public void PlayAs(Country country){
			PlayerCountry = country;
			selectedHistory.Clear();
			selectHistoryCount = 0;
			HasPlayerCountryChanged = true;
			hud.RefreshCountry();
			Refresh();
			HasPlayerCountryChanged = false;
			if (country == null){
				return;
			}
			cameraMovement.SetZoom(cameraMovement.MaxZoom, mainCamera.WorldToScreenPoint(country.Capital.Province.WorldPosition));
		}
		
		public void Refresh(){
			hud.RefreshResources();
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
		#endregion
		
		#region On Click
		private void Select(ISelectable selectable){
			SelectWithoutHistoryUpdate(selectable);
			UpdateSelectedHistory();
		}
		private void SelectWithoutHistoryUpdate(ISelectable selectable){
			if (Selected == selectable){
				return;
			}
			Selected = selectable;
			// Delay the push until after the next OnUpdate() so the current window can remove itself first.
			if (Selected is Province){
				Push(provinceWindow);
			} else if (Selected is Country){
				Push(countryWindow);
			} else if (Selected is Simulation.Military.Regiment){
				Push(regimentWindow);
			} else if (Selected is Simulation.Military.Ship){
				Push(shipWindow);
			}
		}
		public void Deselect(ISelectable selectable){
			if (Selected != selectable){
				return;
			}
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
		#endregion
		
		#region Getting Functions
		public UILayer GetLayerBelow(UILayer layer){
			StackList.RemoveAll(l => l == null);
			int index = StackList.FindIndex(l => l == layer);
			if (index < 1){
				return null;
			}
			return StackList[index-1];
		}
		public Simulation.Military.Harbor GetHarbor(Province province){
			if (!province.IsCoast){
				return null;
			} 
			Simulation.Military.Harbor closestHarbor = null;
			float closestSquareDistance = float.MaxValue;
			foreach (ProvinceLink provinceLink in province.Links){
				if (provinceLink is not ShallowsLink shallowsLink){
					continue;
				}
				Simulation.Military.Harbor harbor = shallowsLink.Harbor;
				float squareDistance = (harbor.WorldPosition-mouseWorldPosition).sqrMagnitude;
				if (closestSquareDistance < squareDistance){
					continue;
				}
				closestHarbor = harbor;
				closestSquareDistance = squareDistance;
			}
			// All coastal provinces have at least one harbor.
			Debug.Assert(closestHarbor != null);
			return closestHarbor;
		}
		#endregion
	}
}
