using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Player {
    [RequireComponent(typeof(CameraMovement))]
    public class CameraInput : MonoBehaviour {
        private static readonly Rect UnitRect = new(0, 0, 1, 1);
        private Input.CameraActions input;

        public CameraMovement Movement {get; private set;}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal DebugConsole DebugConsole {private get; set;}
#endif
        
        private void Awake(){
            Movement = GetComponent<CameraMovement>();
            input = new Input().Camera;
            input.Enable();

            Action<InputAction.CallbackContext> directionalMovement = context => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (DebugConsole.IsKeyboardBusy){
                    return;
                }
#endif
                Movement.DirectionalMovement(context.ReadValue<Vector2>());
            };
            input.DirectionalMovement.performed += directionalMovement;
            input.DirectionalMovement.canceled += directionalMovement;
            
            input.ScrollWheel.performed += async context => {
                float value = context.ReadValue<Vector2>().y;
                await Task.Yield();
                if (!EventSystem.current.IsPointerOverGameObject() && UnitRect.Contains(Movement.Camera.ScreenToViewportPoint(input.MousePosition.ReadValue<Vector2>()))){
                    Movement.ChangeZoom(Mathf.RoundToInt(value));
                }
            };
            
            input.MiddleClick.performed += _ => {
                Movement.StartDragging();
            };
            input.MiddleClick.canceled += _ => {
                Movement.ReleaseDragging();
            };
            
            input.MousePosition.performed += context => {
                Movement.UpdateMousePosition(context.ReadValue<Vector2>());
            };
        }
        public void SetMapMode(int index){
            Movement.SetMapMode(index);
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update(){
            if (DebugConsole.IsKeyboardBusy){
                Movement.DirectionalMovement(Vector2.zero);
            } else {
                Movement.DirectionalMovement(input.DirectionalMovement.ReadValue<Vector2>());
            }
        }
#endif
    }
}
