using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player {
    [RequireComponent(typeof(CameraMovement))]
    public class CameraInput : MonoBehaviour {
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

            input.ScrollWheel.performed += context => {
                Movement.ChangeZoom(Mathf.RoundToInt(context.ReadValue<Vector2>().y));
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
