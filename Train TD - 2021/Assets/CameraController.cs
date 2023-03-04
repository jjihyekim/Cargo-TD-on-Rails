using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    public static CameraController s;

    private void Awake() {
        s = this;
    }

    public Camera mainCamera {
        get {
            return LevelReferences.s.mainCam;
        }
    }

    public Transform cameraCornerBottomLeft;
    public Transform cameraCornerTopRight;


    public Transform cameraCenter;
    public Transform cameraOffset;
    public Transform cameraOffsetFlat;

    public bool isRight = true;

    public float edgeScrollMoveSpeed = 8f;
    public float wasdSpeed = 8f;
    public float snappedwasdDelay = 0.5f; 
    public float zoomSpeed = 0.1f;
    public bool canZoom = true;
    public float middleMoveSpeed = 2f;

    public float posLerpSpeed = 1f;
    public float rotationAngle = 50;
    public float rotationAngleTarget = 50;
    public float minAngle = 0;
    public float maxAngle = 120;
    public float snapToDefaultAngleDistance = 5;
    public float rotLerpSpeed = 20f;

    public InputActionProperty moveAction;
    public InputActionProperty rotateAction;
    public InputActionProperty zoomAction;
    public InputActionProperty rotateCameraAction;
    

    public float currentZoom = 0;
    public float realZoom = 0f;
    public Vector2 zoomLimit = new Vector2(-2, 2);

    public float snapZoomCutoff = 2f;

    public bool isSnappedToTrain = false;
    public bool isSnappedToTransform = false;
    public Transform snapTarget;
    public float minSnapDistance = 2f;

    public bool canEdgeMove = false;
    
    protected void OnEnable()
    {
        moveAction.action.Enable();
        rotateAction.action.Enable();
        zoomAction.action.Enable();
        rotateCameraAction.action.Enable();
        rotateAction.action.performed += FlipCamera;
    }


    protected void OnDisable()
    { 
        moveAction.action.Disable();
        rotateAction.action.Disable();
        zoomAction.action.Disable();
        rotateCameraAction.action.Disable();
        rotateAction.action.performed -= FlipCamera;
    }

    public GameObject cameraLerpDummy;
    public GameObject cameraShakeDummy;
    private void Start() {
#if UNITY_EDITOR
        edgeScrollMoveSpeed = 0; // we dont want edge scroll in the editor
#endif
        cameraCenter.transform.rotation = Quaternion.Euler(0, isRight ? -rotationAngleTarget : rotationAngleTarget, 0);
        SetMainCamPos();
    }

    private bool snappedToTrainLastFrame = false;

    public UnityEvent AfterCameraPosUpdate = new UnityEvent();
    private void LateUpdate() {
        if (directControlActive) {
            ProcessDirectControl(moveAction.action.ReadValue<Vector2>());
        } else {
            var mousePos = Mouse.current.position.ReadValue();
            if (canEdgeMove)
                ProcessScreenCorners(mousePos);

            if (!isSnappedToTransform)
                ProcessMovementInput(moveAction.action.ReadValue<Vector2>(), wasdSpeed);
            else
                ProcessMovementSnapped(moveAction.action.ReadValue<Vector2>(), snappedwasdDelay);

            if (canZoom)
                ProcessZoom(zoomAction.action.ReadValue<float>());

            ProcessMiddleMouseRotation(rotateCameraAction.action.ReadValue<float>(), mousePos);

            LerpCameraTarget();
        }
        

        SetMainCamPos();
        AfterCameraPosUpdate?.Invoke();
    }

    private Vector2 mousePosLastFrame;
    private void ProcessMiddleMouseRotation(float click, Vector2 mousePos) {
        if (click > 0.5f) {
            var delta = mousePosLastFrame.x-mousePos.x;
            /*if (!isRight)
                delta = -delta;*/
            rotationAngleTarget += delta * middleMoveSpeed * Time.unscaledDeltaTime;

            isRight = rotationAngleTarget > 0;

            rotationAngleTarget = Mathf.Clamp(rotationAngleTarget, minAngle, maxAngle);
        } else {
            if (Mathf.Abs(Mathf.Abs(rotationAngleTarget) - rotationAngle) < snapToDefaultAngleDistance) {
                rotationAngleTarget = Mathf.MoveTowards(rotationAngleTarget, isRight? rotationAngle : -rotationAngle, 10 * Time.unscaledDeltaTime);
            }
        }

        mousePosLastFrame = mousePos;
    }

    private void LerpCameraTarget() {
        //var centerRotTarget = Quaternion.Euler(0, isRight ? -rotationAngleTarget : rotationAngleTarget, 0);
        var centerRotTarget = Quaternion.Euler(0, -rotationAngleTarget, 0);

        cameraCenter.transform.rotation = Quaternion.Lerp(cameraCenter.transform.rotation, centerRotTarget, rotLerpSpeed * Time.unscaledDeltaTime);

        var targetPos = cameraOffset.position + cameraOffset.forward * currentZoom;
        var targetRot = cameraOffset.rotation;
        cameraLerpDummy.transform.position = Vector3.Lerp(cameraLerpDummy.transform.position, targetPos, posLerpSpeed * Time.unscaledDeltaTime);
        cameraLerpDummy.transform.rotation = Quaternion.Lerp(cameraLerpDummy.transform.rotation, targetRot, rotLerpSpeed * Time.unscaledDeltaTime);

        // lerp affected real zoom
        realZoom = Vector3.Distance(cameraLerpDummy.transform.position, cameraOffset.position);
        if (currentZoom < 0)
            realZoom = -realZoom;
    }

    void SetMainCamPos() {
        mainCamera.transform.position = cameraShakeDummy.transform.position;
        mainCamera.transform.rotation = cameraShakeDummy.transform.rotation;
    }

    void ProcessZoom(float value) {
        currentZoom += value * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, zoomLimit.x, zoomLimit.y);
        if (Mathf.Abs(value) > 0.1) {
            CancelInvoke(nameof(SnapZoom));
            Invoke(nameof(SnapZoom), 0.7f);
        }
    }

    void SnapZoom() {
        if (Mathf.Abs(currentZoom) < 1.25) {
            currentZoom = 0;
        }

        if (Mathf.Abs(currentZoom - snapZoomCutoff) < 1.25) {
            currentZoom = snapZoomCutoff;
        }
    }

    public float cameraScrollEdgePercent = 0.01f;
    void ProcessScreenCorners(Vector2 mousePosition) {
        float xPercent = mousePosition.x / Screen.width;
        float yPercent = mousePosition.y / Screen.height;
        var scrollXReq = cameraScrollEdgePercent;
        var scrollYReq = cameraScrollEdgePercent * Screen.height / Screen.width;

        var output = Vector2.zero;

        if (xPercent < scrollXReq) {
            output.x = -1f;
        }else if (xPercent > 1-scrollXReq) {
            output.x = 1f;
        }
        
        if (yPercent < scrollYReq) {
            output.y = -1f;
        }else if (yPercent > 1-scrollYReq) {
            output.y = 1f;
        }
        
        ProcessMovementInput(output, edgeScrollMoveSpeed);
    }

    [Header("Map Settings")]
    
    public bool isSnappedToMap = false;
    public Transform mapCameraCornerBottomLeft;
    public Transform mapCameraCornerTopRight;
    public Vector3 mapStartPos = new Vector3(100, 0, -100);
    public Vector3 mapPos;
    private Vector3 regularPos;
    private float mapZoom;
    private float regularZoom;

    public void ResetMapPos() {
        mapPos = mapStartPos;
        mapZoom = 0;
    }

    public void SetMapPos(Vector3 position) {
        position.y = 0;
        mapPos = position;
    }

    public void EnterMapMode() {
        isSnappedToMap = true;
        regularPos = cameraCenter.position;
        cameraCenter.position = mapPos;
        regularZoom = currentZoom;
        currentZoom = mapZoom;
        if (!isRight) {
            FlipCamera(new InputAction.CallbackContext());
        }
    }

    public void ExitMapMode() {
        isSnappedToMap = false;
        mapPos = cameraCenter.position;
        cameraCenter.position = regularPos;
        mapZoom = currentZoom;
        currentZoom = regularZoom;
        if (!isRight) {
            FlipCamera(new InputAction.CallbackContext());
        }
    }

    void ProcessMovementInput(Vector2 value, float multiplier) {
        var delta = new Vector3(value.x, 0, value.y);
        
        var transformed = cameraOffsetFlat.TransformDirection(delta);
        
        
        var camPos = cameraCenter.position;
        
        camPos += transformed * multiplier * Time.unscaledDeltaTime;

        Vector3 topRight = isSnappedToMap ? mapCameraCornerTopRight.position : cameraCornerTopRight.position;
        Vector3 bottomLeft = isSnappedToMap ? mapCameraCornerBottomLeft.position : cameraCornerBottomLeft.position;
        
        if (!isSnappedToMap) {
            var len = ((Train.s.cartCount-3) * DataHolder.s.cartLength)/2;
            topRight.z += len;
            bottomLeft.z -= len;
        }
        
        if (camPos.x < bottomLeft.x) {
            camPos = new Vector3(bottomLeft.x, camPos.y, camPos.z);
        }else if (camPos.x > topRight.x) {
            camPos = new Vector3(topRight.x, camPos.y, camPos.z);
        }
        
        if (camPos.z < bottomLeft.z) {
            camPos = new Vector3(camPos.x, camPos.y, bottomLeft.z);
        }else if (camPos.z > topRight.z) {
            camPos = new Vector3(camPos.x, camPos.y, topRight.z);
        }

        cameraCenter.position = camPos;
    }
    
    

    private float snappedMoveTimer = 0;
    public float snappedMovementLerp = 1f;
    void ProcessMovementSnapped(Vector2 value, float delay) {
        if (snapTarget == null) {
            UnSnap();
            return;
        }
        
        cameraCenter.position = Vector3.Lerp(cameraCenter.position, snapTarget.position + snapOffset, snappedMovementLerp*Time.unscaledDeltaTime);

        if (isSnappedToTrain) {
            if (snappedMoveTimer <= 0) {
                if (Mathf.Abs(value.x) > 0.1f) {
                    var nextBuilding = GetNextBuildingInSameSlot(value.x > 0, snappedTrainBuilding.mySlot, snappedTrainBuilding.mySlotIndex);
                    if (nextBuilding != null) {
                        SnapToTrainModule(nextBuilding);
                        PlayerModuleSelector.s.ActivateActionDisplayOnTrainBuilding(snappedTrainBuilding);
                    }
                    snappedMoveTimer = delay;
                }

                if (Mathf.Abs(value.y) > 0.1f) {
                    var nextBuilding = GetNextBuildingInTheNextSlot(value.y > 0, snappedTrainBuilding.mySlot, snappedTrainBuilding.mySlot.GetCart().index);
                    
                    if (nextBuilding != null) {
                        SnapToTrainModule(nextBuilding);
                        PlayerModuleSelector.s.ActivateActionDisplayOnTrainBuilding(snappedTrainBuilding);
                    }
                    
                    snappedMoveTimer = delay;
                }

            }
        } else {
            isSnappedToTransform = false;
        }

        if (value.sqrMagnitude < 0.1f) {
            snappedMoveTimer = 0;
        }

        snappedMoveTimer -= Time.unscaledDeltaTime;
    }

    TrainBuilding GetNextBuildingInSameSlot(bool isForward, Slot activeSlot, int activeIndex) {
        var slotCount = activeSlot.myBuildings.Length;
        if (isForward) {
            var nextIndex = activeIndex;
            for (int i = 0; i < slotCount; i++) {
                nextIndex = (nextIndex + 1) % slotCount;

                var nextBuilding = activeSlot.myBuildings[nextIndex];
                if (nextBuilding != null && nextBuilding != snappedTrainBuilding && nextBuilding.canSelect) {
                    return activeSlot.myBuildings[nextIndex];
                }
            }
        } else {
            var nextIndex = activeIndex;
            for (int i = 0; i < slotCount; i++) {
                nextIndex = (nextIndex + (slotCount-1)) % slotCount; // +2 actually makes us go -1 because modulo 3

                var nextBuilding = activeSlot.myBuildings[nextIndex];
                if (nextBuilding != null && nextBuilding != snappedTrainBuilding && nextBuilding.canSelect) {
                    return activeSlot.myBuildings[nextIndex];
                }
            }
        }
        
        return null;
    }
    
    TrainBuilding GetNextBuildingInTheNextSlot(bool isForward, Slot activeSlot, int slotIndex) {
        var cartCount = Train.s.carts.Count;
        if (isForward) {
            var nextIndex = slotIndex;
            for (int i = 0; i < cartCount; i++) {
                
                if (!activeSlot.isFrontSlot) {
                    activeSlot = activeSlot.GetCart().frontSlot;
                } else {
                    nextIndex = (nextIndex + (cartCount-1)) % cartCount; // +2 actually makes us go -1 because modulo 3
                    activeSlot = Train.s.carts[nextIndex].GetComponent<Cart>().backSlot;
                }

                var buildingInSlot = GetNextBuildingInSameSlot(true, activeSlot, -1);

                if (buildingInSlot != null && buildingInSlot.canSelect) {
                    return buildingInSlot;
                }
            }
        } else {
            var nextIndex = slotIndex;
            for (int i = 0; i < cartCount; i++) {
                
                if (activeSlot.isFrontSlot) {
                    activeSlot = activeSlot.GetCart().backSlot;
                } else {
                    nextIndex = (nextIndex + 1) % cartCount;
                    activeSlot = Train.s.carts[nextIndex].GetComponent<Cart>().frontSlot;
                }

                var buildingInSlot = GetNextBuildingInSameSlot(true, activeSlot, -1);

                if (buildingInSlot != null && buildingInSlot.canSelect) {
                    return buildingInSlot;
                }
            }
        }
        return null;
    }

    /*public bool SnapToNearestCart() {
        var carts = Train.s.carts;
        var minDist = float.MaxValue;
        if (!snappedToTrainLastFrame) {
            targetCart = -1;

            for (int i = 0; i < carts.Count; i++) {
                var dist = Vector3.Distance(cameraCenter.position, carts[i].position);

                if (dist < minDist) {
                    targetCart = i;
                    minDist = dist;
                }
            }
        }
        
        return minDist < minSnapDistance;
    }*/

    public TrainBuilding snappedTrainBuilding;
    public Vector3 snapOffset;
    public void SnapToTrainModule(TrainBuilding module) {
        if (module.myRotation == TrainBuilding.Rots.right && isRight) {
            FlipCamera(new InputAction.CallbackContext());
        }else if (module.myRotation == TrainBuilding.Rots.left && !isRight) {
            FlipCamera(new InputAction.CallbackContext());
        }

        snappedTrainBuilding = module;
        snapTarget = module.transform;
        snapOffset = Vector3.down * 0.75f;
        isSnappedToTransform = true;
        isSnappedToTrain = true;
    }

    public void SnapToTransform(Transform target) {
        snapTarget = target;
        snapOffset = Vector3.down * 0.5f;
        isSnappedToTransform = true;
        isSnappedToTrain = false;
    }

    public void UnSnap() {
        isSnappedToTransform = false;
        isSnappedToTrain = false;
    }



    public void FlipCamera(InputAction.CallbackContext info) {
        isRight = !isRight;
        rotationAngleTarget = isRight? rotationAngle : -rotationAngle;
    }

    
    public void ToggleCameraEdgeMove() {
        canEdgeMove = !canEdgeMove;
    }

    [Header("Direct Control Settings")] 
    public bool directControlActive = false;
    public Transform directControlTransform;
    private Vector2 rotTarget;
    public float mouseSensitivity = 1f;
    public float gamepadSensitivity = 1f;
    public float overallSensitivity = 1f;
    private bool rotLerping = true;

    public void ProcessDirectControl(Vector2 stickInput) {
        var realInput = stickInput*gamepadSensitivity + Mouse.current.delta.ReadValue() * mouseSensitivity;
        realInput *= Time.unscaledDeltaTime * (overallSensitivity/2.5f);

        rotTarget += realInput;

        Quaternion xQuaternion = Quaternion.AngleAxis (rotTarget.x, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis (rotTarget.y, -Vector3.right);
        
        var targetPos = directControlTransform.position;
        var targetRot = directControlTransform.rotation * xQuaternion * yQuaternion;
        cameraLerpDummy.transform.position = Vector3.Lerp(cameraLerpDummy.transform.position, targetPos, posLerpSpeed * Time.unscaledDeltaTime);

        if (rotLerping) {
            cameraLerpDummy.transform.rotation = Quaternion.Lerp(cameraLerpDummy.transform.rotation, targetRot, rotLerpSpeed * Time.unscaledDeltaTime);
            if (Quaternion.Angle(cameraLerpDummy.transform.rotation, targetRot) < 5) {
                rotLerping = false;
            }
        } else {
            cameraLerpDummy.transform.rotation = targetRot;
        }
    }
    
    public void ActivateDirectControl(Transform target) {
        directControlTransform = target;
        directControlActive = true;
        rotTarget = Vector2.zero;
        rotLerping = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void DisableDirectControl() {
        directControlActive = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ManualRotateDirectControl(float amount) {
        rotTarget.x += amount/1.2f;
    }
}
