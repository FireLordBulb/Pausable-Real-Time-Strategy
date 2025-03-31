using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Player {
    [RequireComponent(typeof(CameraMovement))]
    public class CameraInput : MonoBehaviour {
        private CameraMovement cameraMovement;
        private Input.CameraActions input;
        private void Awake(){
            cameraMovement = GetComponent<CameraMovement>();
            input = new Input().Camera;
            input.Enable();

            Action<InputAction.CallbackContext> directionalMovement = context => {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (EventSystem.current.currentSelectedGameObject != null){
                    return;
                }
#endif
                cameraMovement.DirectionalMovement(context.ReadValue<Vector2>());
            };
            input.DirectionalMovement.performed += directionalMovement;
            input.DirectionalMovement.canceled += directionalMovement;

            input.ScrollWheel.performed += context => {
                cameraMovement.ChangeZoom(Mathf.RoundToInt(context.ReadValue<Vector2>().y));
            };
            
            input.MiddleClick.performed += _ => {
                cameraMovement.StartDragging();
            };
            input.MiddleClick.canceled += _ => {
                cameraMovement.ReleaseDragging();
            };

            input.MousePosition.performed += context => {
                cameraMovement.UpdateMousePosition(context.ReadValue<Vector2>());
            };
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update(){
            if (EventSystem.current.currentSelectedGameObject != null){
                cameraMovement.DirectionalMovement(Vector2.zero);
            } else {
                cameraMovement.DirectionalMovement(input.DirectionalMovement.ReadValue<Vector2>());
            }
        }
#endif
    }
}
