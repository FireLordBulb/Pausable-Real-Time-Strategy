using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour {
    [SerializeField] private float[] zoomLevels;
    [SerializeField] private float zoomChangeSeconds;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stoppingSeconds;

    private new Camera camera;
    private Input.CameraActions input;
    
    private Vector2 movementDirection;
    private int targetZoom;
    private bool isDragging;
    
    private int previousZoom;
    private Vector3 zoomStartMousePosition;
    private Vector3 lockedMousePoint;
    private Vector2 movementVelocity;

    private bool IsMouseLocked => isDragging || previousZoom != targetZoom;
    private Ray MouseRay => camera.ScreenPointToRay(input.MousePosition.ReadValue<Vector2>());
    private Vector3 MousePosition => input.MousePosition.ReadValue<Vector2>();
    
    private void Awake(){
        camera = GetComponent<Camera>();
        input = new Input().Camera;
        input.Enable();

        Action<InputAction.CallbackContext> directionalMovement = context => {
            movementDirection = context.ReadValue<Vector2>();
        };
        input.DirectionalMovement.performed += directionalMovement;
        input.DirectionalMovement.canceled += directionalMovement;

        input.ScrollWheel.performed += context => {
            zoomStartMousePosition = MousePosition;
            if (Physics.Raycast(camera.ScreenPointToRay(zoomStartMousePosition), out RaycastHit hit)){
                lockedMousePoint = hit.point;
            }
            
            int targetZoomSaved = targetZoom;
            targetZoom += Mathf.RoundToInt(context.ReadValue<Vector2>().y);
            targetZoom = Mathf.Clamp(targetZoom, 0, zoomLevels.Length-1);
            // Inverting the direction mid-zoom.
            if (targetZoom == previousZoom){
                previousZoom = targetZoomSaved;
            }
        };

        input.MiddleClick.performed += _ => {
            if (!Physics.Raycast(MouseRay, out RaycastHit hit)){
                return;
            }
            isDragging = true;
            lockedMousePoint = hit.point;
        };
        input.MiddleClick.canceled += _ => {
            isDragging = false;
            zoomStartMousePosition = MousePosition;
        };
        
        // TODO: Make zoom level start init based on starting y-position in scene.
        
        // TODO: Change rotation based on zoom level.
    }
    private void Update(){
       Vector3 position = transform.position;
        
        float zoomStep = (zoomLevels[targetZoom]-zoomLevels[previousZoom])*Time.deltaTime/zoomChangeSeconds;
        float newY = Mathf.MoveTowards(position.y, zoomLevels[targetZoom], Mathf.Abs(zoomStep));
        float sign = Mathf.Sign(zoomStep);
        if (newY*sign >= zoomLevels[targetZoom]*sign){
            previousZoom = targetZoom;
        } else {
            position.y = newY;
        }
        
        if (movementDirection != Vector2.zero){
            movementVelocity = movementSpeed*movementDirection;
        } else if (movementVelocity != Vector2.zero){
            Vector2 velocityLoss = movementVelocity.normalized*Time.deltaTime/stoppingSeconds;
            if (movementVelocity.sqrMagnitude < velocityLoss.sqrMagnitude){
                movementVelocity = Vector2.zero;
            } else {
                movementVelocity -= velocityLoss;
            }
        }
        // Scale the velocity with the y so that the movement speed is relative to the zoom.
        Vector3 positionDelta = ToXZPlane(Time.deltaTime*position.y*movementVelocity);
        if (IsMouseLocked){
            lockedMousePoint += positionDelta;
            if (Physics.Raycast(isDragging ? MouseRay : camera.ScreenPointToRay(zoomStartMousePosition), out RaycastHit hit)){
                position += lockedMousePoint-hit.point;
            }
        } else {
            position += positionDelta;
        }

        transform.position = position;
    }
    
    private static Vector3 ToXZPlane(Vector2 vector) => new(vector.x, 0, vector.y);
}
