using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour {
    [SerializeField] private float[] zoomLevels;
    [SerializeField] private float zoomChangeSeconds;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float stoppingSeconds;
    
    private Input.CameraActions input;
    
    private Vector2 movementDirection;
    private int targetZoom;

    private int previousZoom;
    private Vector2 movementVelocity;
    
    private void Awake(){
        input = new Input().Camera;
        input.Enable();

        Action<InputAction.CallbackContext> directionalMovement = context => {
            movementDirection = context.ReadValue<Vector2>();
        };
        input.DirectionalMovement.performed += directionalMovement;
        input.DirectionalMovement.canceled += directionalMovement;

        input.ScrollWheel.performed += context => {
            int targetZoomSaved = targetZoom;
            targetZoom += Mathf.RoundToInt(context.ReadValue<Vector2>().y);
            targetZoom = Mathf.Clamp(targetZoom, 0, zoomLevels.Length-1);
            // Inverting the direction mid-zoom.
            if (targetZoom == previousZoom){
                previousZoom = targetZoomSaved;
            }
        };
        
        // TODO: middle-mouse click and drag
        
        // TODO: Make zoom centered around mouse position.
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
        Vector2 positionDelta = Time.deltaTime*transform.position.y*movementVelocity;
        position += new Vector3(positionDelta.x, 0, positionDelta.y);

        transform.position = position;
    }
}
