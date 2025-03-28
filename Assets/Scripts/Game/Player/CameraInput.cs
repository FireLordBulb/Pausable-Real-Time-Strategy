using System;
using UnityEngine;
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
    }
}
