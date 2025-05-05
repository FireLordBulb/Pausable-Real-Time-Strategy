using System;
using System.Collections.Generic;
using ActionStackSystem;
using AI;
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
		[SerializeField] private SelectionWindow[] selectionWindows;
		[Space]
		[SerializeField] private Button closeButton;
		[SerializeField] private LayerMask mapClickMask;
		[SerializeField] private int maxSelectHistory;
		#endregion
		#region Private Fields
		private Input.UIActions input;
		private readonly Dictionary<Type, SelectionWindow> selectionWindowMap = new();
		private Canvas canvas;
		private SelectionWindow activeSelectionWindow;
		private CameraMovement cameraMovement;
		private Camera mainCamera;
		private readonly Dictionary<Country, AIController> aiControllers = new();
		private AIController playerAI;
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
		public CameraInput CameraInput => cameraInput;
		private Vector3 MousePosition => input.MousePosition.ReadValue<Vector2>();
		#endregion

		#region Awake
		private void Awake(){
			InitScene();
			SpawnUI();
			InitAI();
			EnableInput();
		}
		private void InitScene(){
			canvas = GetComponent<Canvas>();
			map = Instantiate(map);
			map.gameObject.name = "Map";
			cameraInput = Instantiate(cameraInput);
			cameraInput.gameObject.name = "MainCamera";
			cameraMovement = cameraInput.Movement;
			cameraMovement.Map = map;
			mainCamera = cameraMovement.Camera;
		}	
		private void SpawnUI(){
			Links = new Links(selectable => {
				if (Selected != selectable){
					Select(selectable);
				} else {
					Deselect(selectable);
				}
			});
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
			foreach (SelectionWindow selectionWindow in selectionWindows){
				selectionWindowMap.Add(selectionWindow.TargetType, selectionWindow);
			}
			map.Calendar.OnMonthTick.AddListener(Refresh);
		}
		private void InitAI(){
			foreach (Country country in map.Countries){
				AIController aiController = country.GetComponent<AIController>();
				aiController.Init();
				aiControllers.Add(country, aiController);
			}
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
			input.F1.performed += _ => FunctionKey(0);
			input.F2.performed += _ => FunctionKey(1);
			input.F3.performed += _ => FunctionKey(2);
			input.F4.performed += _ => FunctionKey(3);
			input.F5.performed += _ => FunctionKey(4);
			input.F6.performed += _ => FunctionKey(5);
			input.F7.performed += _ => FunctionKey(6);
			input.F8.performed += _ => FunctionKey(7);
			input.F9.performed += _ => {
				if (IsControlHeld){
					canvas.targetDisplay = canvas.targetDisplay == -1 ? 0: -1;
				} else {
					FunctionKey(8);
				}
			};
			input.F10.performed += _ => FunctionKey(9);
			input.F11.performed += _ => FunctionKey(10);
			input.F12.performed += _ => FunctionKey(11);

			input.MapMode0.performed += _ => SelectMapMode(0);
			input.MapMode1.performed += _ => SelectMapMode(1);
			input.MapMode2.performed += _ => SelectMapMode(2);
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

		private void FunctionKey(int index){
			if (IsControlHeld){
				return;
			}
			hud.OpenSidePanel(index);
		}
		private void SelectMapMode(int index){
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (debugConsole.IsKeyboardBusy){
				return;
			}
#endif
			hud.MapModeSelect.Select(index);
		}
		#endregion
		
		#region Update
		protected override void Update(){
			UpdateHover();
			base.Update();
			PushUninstantiatedSelectionWindow();
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
		// ReSharper disable Unity.PerformanceAnalysis // This is called in update but the if-statement blocks the performance-intensive part from being called in sequential frames.
		private void PushUninstantiatedSelectionWindow(){
			if (activeSelectionWindow != null && !activeSelectionWindow.gameObject.scene.IsValid()){
				activeSelectionWindow = Push(activeSelectionWindow);
			}
		}
		#endregion

		#region Public Void Methods
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
		public void PlayAs(Country country){
			if (playerAI != null){
				playerAI.enabled = true;
			}
			PlayerCountry = country;
			if (PlayerCountry != null){
				playerAI = GetAI(PlayerCountry);
				playerAI.enabled = false;
			}
			Selected = null;
			activeSelectionWindow = null;
			selectedHistory.Clear();
			selectHistoryCount = 0;
			HasPlayerCountryChanged = true;
			hud.RefreshCountry();
			Refresh();
			HasPlayerCountryChanged = false;
			if (country == null){
				return;
			}
			if (country.enabled){
				cameraMovement.SetZoom(cameraMovement.MaxZoom, mainCamera.WorldToScreenPoint(country.Capital.Province.WorldPosition));
			}
		}
		public void Refresh(){
			hud.RefreshResources();
			if (activeSelectionWindow != null){
				activeSelectionWindow.Refresh();
			}
		}
		#endregion
		
		#region Clicking to Select
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
				if (CurrentAction != null){
					Select(CurrentAction.OnSelectableClicked(mouseDownSelectable, isRightClick));
				}
			}
			mouseDownSelectable = null;
		}
		private void Select(ISelectable selectable){
			SelectWithoutHistoryUpdate(selectable);
			UpdateSelectedHistory();
		}
		private void SelectWithoutHistoryUpdate(ISelectable selectable){
			if (Selected == selectable){
				return;
			}
			Selected = selectable;
			if (Selected != null){
				Type type = Selected.GetType();
				while (type != null && !selectionWindowMap.TryGetValue(type, out activeSelectionWindow)){
					type = type.BaseType;
				}
			} else {
				activeSelectionWindow = null;
			}
		}
		public void Deselect(ISelectable selectable){
			if (Selected != selectable){
				return;
			}
			Selected = null;
			activeSelectionWindow = null;
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
		
		#region Getter Functions
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
		public AIController GetAI(Country country){
			return aiControllers[country];
		}
		#endregion
	}
}
