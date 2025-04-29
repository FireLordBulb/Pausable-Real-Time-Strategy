using System;
using Mathematics;
using Simulation;
using UnityEngine;

namespace Player {
    [RequireComponent(typeof(Camera))]
    public class CameraMovement : MonoBehaviour {
        private const float Opaque = 1;
        private static readonly Vector3 Center = new(0.5f, 0.5f);
        
        [Tooltip("In order of largest to smallest height above map.")]
        [SerializeField] private ZoomLevel[] zoomLevels;
        [SerializeField] private float zoomChangeSeconds;
        [SerializeField] private float movementSpeed;
        [SerializeField] private float stoppingSeconds;
        [SerializeField] private float terrainMapModeAlpha;
        [SerializeField] private LayerMask raycastMask;
        
        private Vector2 movementDirection;
        private int targetZoom;
        private bool isDragging;
        private Vector3 mousePosition;
        
        private int previousZoom;
        // Well, smaller or equal.
        private int nearestSmallerZoom;
        private Vector3 zoomStartMousePosition;
        private Vector3 lockedMousePoint;
        private Vector2 movementVelocity;
        private float currentAlpha;

        public Camera Camera {get; private set;}
        public MapGraph Map {private get; set;}
        
        public int MaxZoom => zoomLevels.Length-1;
        private bool IsMouseLocked => isDragging || previousZoom != targetZoom;
        private Ray MouseRay => Camera.ScreenPointToRay(mousePosition);
        
        private void Awake(){
            Camera = GetComponent<Camera>();

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
                zoomStartMousePosition = Camera.ViewportToScreenPoint(Center);
                break;
            }
        }

        public void DirectionalMovement(Vector2 direction){
            movementDirection = direction;
        }
        public void ChangeZoom(int change){
            SetZoom(targetZoom+change);
        }
        public void SetZoom(int zoom){
            SetZoom(zoom, mousePosition);
        }
        public void SetZoom(int zoom, Vector3 zoomCenter){
            zoomStartMousePosition = zoomCenter;
            if (Raycast(Camera.ScreenPointToRay(zoomStartMousePosition), out RaycastHit hit)){
                lockedMousePoint = hit.point;
            }
                
            int targetZoomSaved = targetZoom;
            targetZoom = zoom;
            targetZoom = Mathf.Clamp(targetZoom, 0, zoomLevels.Length-1);
                
            // Inverting the direction mid-zoom.
            // Both of differences being negative or both being positive means y isn't between target and
            // previous, requiring previous to be changed to prevent a discontinuous jump.
            float y = transform.position.y;
            if (Math.Sign(y-zoomLevels[previousZoom].height)*Math.Sign(y-zoomLevels[targetZoom].height) != -1){
                previousZoom = targetZoomSaved;
            }
        }
        public void StartDragging(){
            if (!Raycast(MouseRay, out RaycastHit hit)){
                return;
            }
            isDragging = true;
            lockedMousePoint = hit.point;
        }
        public void ReleaseDragging(){
            isDragging = false;
            zoomStartMousePosition = mousePosition;
        }
        public void UpdateMousePosition(Vector3 position){
            mousePosition = position;
        }
        
        private void SetZoomLevel(int index){
            Vector3 position = transform.position;
            position.y = zoomLevels[index].height;
            transform.position = position;
            targetZoom = nearestSmallerZoom = previousZoom = index;
            currentAlpha = zoomLevels[index].doUseTerrainMapMode ? terrainMapModeAlpha : Opaque;
        }
        
        private void Update(){
            float previousAlpha = currentAlpha;
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
                    nearestSmallerZoom = Mathf.Min(nearestSmallerZoom+1, zoomLevels.Length-1);
                } else if (nearestSmallerZoom != 0){
                    if (newY >= zoomLevels[nearestSmallerZoom-1].height){
                        nearestSmallerZoom--;
                    }
                }
            }
            // Rotation & Map Opacity setting --------------------------------------------------------
            if (nearestSmallerZoom == 0){
                transform.rotation = zoomLevels[nearestSmallerZoom].rotation;
                currentAlpha = Opaque;
            } else{
                ZoomLevel smallerZoom = zoomLevels[nearestSmallerZoom];
                ZoomLevel largerZoom = zoomLevels[nearestSmallerZoom-1];
                float slerpParameter = Mathf.InverseLerp(smallerZoom.height, largerZoom.height, position.y);
                transform.rotation = Quaternion.Slerp(smallerZoom.rotation, largerZoom.rotation, slerpParameter);

                if (smallerZoom.doUseTerrainMapMode && !largerZoom.doUseTerrainMapMode){
                    currentAlpha = Mathf.Lerp(terrainMapModeAlpha, Opaque, slerpParameter);
                } else {
                    currentAlpha = Opaque;
                }
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
            Vector3 positionDelta = VectorGeometry.ToXZPlane(Time.deltaTime*position.y*movementVelocity);
            if (IsMouseLocked){
                lockedMousePoint += positionDelta;
                if (Raycast(isDragging ? MouseRay : Camera.ScreenPointToRay(zoomStartMousePosition), out RaycastHit hit)){
                    position += lockedMousePoint-hit.point;
                }
            } else {
                position += positionDelta;
            }
            transform.position = position;
            // Switching between political and terrain map modes by applying opacity to provinces. -----------------
            if (Mathf.Abs(currentAlpha-previousAlpha) < Vector2.kEpsilon){
                return;
            }
            foreach (Province province in Map.LandProvinces){
                province.Alpha = currentAlpha;
            }
        }
        private bool Raycast(Ray ray, out RaycastHit hit){
            hit = new RaycastHit();
            return Physics.Raycast(ray, out hit, float.MaxValue, raycastMask);
        }
        
        [Serializable]
        private struct ZoomLevel {
            public float height;
            public Quaternion rotation;
            public bool doUseTerrainMapMode;
        }
    }
}
