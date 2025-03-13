using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour {
    private static readonly Vector3 Center = new(0.5f, 0.5f);
    
    [Tooltip("In order of largest to smallest height above map.")]
    [SerializeField] private ZoomLevel[] zoomLevels;
    [SerializeField] private float zoomChangeSeconds;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stoppingSeconds;

    private
#if UNITY_EDITOR
            new
#endif
                Camera camera;
    private Input.CameraActions input;
    
    private Vector2 movementDirection;
    private int targetZoom;
    private bool isDragging;
    
    private int previousZoom;
    // Well, smaller or equal.
    private int nearestSmallerZoom;
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
            // Both of differences being negative or both being positive means y isn't between target and
            // previous, requiring previous to be changed to prevent a discontinuous jump.
            float y = transform.position.y;
            if (Math.Sign(y-zoomLevels[previousZoom].height)*Math.Sign(y-zoomLevels[targetZoom].height) != -1){
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

        Vector3 position = transform.position;
        if (position.y < zoomLevels[^1].height){
            SetZoomLevel(zoomLevels.Length-1);
        } else if (position.y > zoomLevels[0].height){
            SetZoomLevel(0);
        } else for (int i = 0; i < zoomLevels.Length; i++){
            if (position.y < zoomLevels[i].height){
                continue;
            }
            nearestSmallerZoom = targetZoom = i;
            previousZoom = Mathf.Max(i-1, 0);
            zoomStartMousePosition = camera.ViewportToScreenPoint(Center);
            break;
        }
    }
    private void SetZoomLevel(int index){
        Vector3 position = transform.position;
        position.y = zoomLevels[index].height;
        transform.position = position;
        targetZoom = nearestSmallerZoom = previousZoom = index;
    }
    
    private void Update(){
        Vector3 position = transform.position;
        // Height --------------------------------------------------------------------------------
        float targetHeight = zoomLevels[targetZoom].height;
        float zoomStep = (targetHeight-zoomLevels[previousZoom].height)*Time.deltaTime/zoomChangeSeconds;
        float newY = Mathf.MoveTowards(position.y, targetHeight, Mathf.Abs(zoomStep));
        float sign = Mathf.Sign(zoomStep);
        if (newY*sign >= targetHeight*sign){
            position.y = targetHeight;
            nearestSmallerZoom = previousZoom = targetZoom;
        } else {
            position.y = newY;
            if (newY < zoomLevels[nearestSmallerZoom].height){
                nearestSmallerZoom++;
            } else if (nearestSmallerZoom != 0){
                if (newY >= zoomLevels[nearestSmallerZoom-1].height){
                    nearestSmallerZoom--;
                }
            }
        }
        // Rotation -----------------------------------------------------------------------------
        if (nearestSmallerZoom == 0){
            transform.rotation = zoomLevels[nearestSmallerZoom].rotation;
        } else {
            ZoomLevel smallerZoom = zoomLevels[nearestSmallerZoom];
            ZoomLevel largerZoom = zoomLevels[nearestSmallerZoom-1];
            float slerpParameter = Mathf.InverseLerp(smallerZoom.height, largerZoom.height, position.y);
            transform.rotation = Quaternion.Slerp(smallerZoom.rotation, largerZoom.rotation, slerpParameter);
        }
        // XZ-plane position --------------------------------------------------------------------
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

    [Serializable]
    private struct ZoomLevel {
        public float height;
        public Quaternion rotation;
    }
}
