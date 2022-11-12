using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
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
    private void LateUpdate() {
        if (directControlActive) {
            ProcessDirectControl(moveAction.action.ReadValue<Vector2>());
        } else {
            var mousePos = Mouse.current.position.ReadValue();
            if (canEdgeMove)
                ProcessScreenCorners(mousePos);

            if (!isSnappedToTrain)
                ProcessMovementInput(moveAction.action.ReadValue<Vector2>(), wasdSpeed);
            else
                ProcessMovementSnapped(moveAction.action.ReadValue<Vector2>(), snappedwasdDelay);

            if (canZoom)
                ProcessZoom(zoomAction.action.ReadValue<float>());

            ProcessMiddleMouseRotation(rotateCameraAction.action.ReadValue<float>(), mousePos);

            LerpCameraTarget();
        }
        

        SetMainCamPos();
    }

    private Vector2 mousePosLastFrame;
    private void ProcessMiddleMouseRotation(float click, Vector2 mousePos) {
        if (click > 0.5f) {
            var delta = mousePos.x-mousePosLastFrame.x;
            if (!isRight)
                delta = -delta;
            rotationAngleTarget += delta * middleMoveSpeed * Time.deltaTime;
            

            rotationAngleTarget = Mathf.Clamp(rotationAngleTarget, minAngle, maxAngle);
        } else {
            if (Mathf.Abs(rotationAngleTarget - rotationAngle) < snapToDefaultAngleDistance) {
                rotationAngleTarget = Mathf.MoveTowards(rotationAngleTarget, rotationAngle, 10 * Time.deltaTime);
            }
        }

        mousePosLastFrame = mousePos;
    }

    private void LerpCameraTarget() {
        var centerRotTarget = Quaternion.Euler(0, isRight ? -rotationAngleTarget : rotationAngleTarget, 0);

        cameraCenter.transform.rotation = Quaternion.Lerp(cameraCenter.transform.rotation, centerRotTarget, rotLerpSpeed * Time.deltaTime);

        var targetPos = cameraOffset.position + cameraOffset.forward * currentZoom;
        var targetRot = cameraOffset.rotation;
        cameraLerpDummy.transform.position = Vector3.Lerp(cameraLerpDummy.transform.position, targetPos, posLerpSpeed * Time.deltaTime);
        cameraLerpDummy.transform.rotation = Quaternion.Lerp(cameraLerpDummy.transform.rotation, targetRot, rotLerpSpeed * Time.deltaTime);

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
            Invoke(nameof(SnapZoom),0.7f);
        }

        if (isSnappedToMap) {
            isSnappedToTrain = false;
        } else {
            if (currentZoom >= snapZoomCutoff) {
                if (!isSnappedToTrain) {
                    SnapToNearestCart();
                }

                isSnappedToTrain = true;
            } else {
                isSnappedToTrain = false;
            }
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

    public void EnterMapMode() {
        isSnappedToMap = true;
        regularPos = cameraCenter.position;
        cameraCenter.position = mapPos;
        regularZoom = currentZoom;
        currentZoom = mapZoom;
    }

    public void ExitMapMode() {
        isSnappedToMap = false;
        mapPos = cameraCenter.position;
        cameraCenter.position = regularPos;
        mapZoom = currentZoom;
        currentZoom = regularZoom;
    }

    void ProcessMovementInput(Vector2 value, float multiplier) {
        var delta = new Vector3(value.x, 0, value.y);
        
        var transformed = cameraOffsetFlat.TransformDirection(delta);
        
        cameraCenter.position += transformed * multiplier * Time.deltaTime;

        Transform topRight = isSnappedToMap ? mapCameraCornerTopRight : cameraCornerTopRight;
        Transform bottomLeft = isSnappedToMap ? mapCameraCornerBottomLeft : cameraCornerBottomLeft;


        var camPos = cameraCenter.position;
        if (camPos.x < bottomLeft.position.x) {
            cameraCenter.position = new Vector3(bottomLeft.position.x, camPos.y, camPos.z);
        }else if (camPos.x > topRight.position.x) {
            cameraCenter.position = new Vector3(topRight.position.x, camPos.y, camPos.z);
        }
        
        if (camPos.z < bottomLeft.position.z) {
            cameraCenter.position = new Vector3(camPos.x, camPos.y, bottomLeft.position.z);
        }else if (camPos.z > topRight.position.z) {
            cameraCenter.position = new Vector3(camPos.x, camPos.y, topRight.position.z);
        }
    }
    
    

    private int targetCart = -1;
    private float snappedMoveTimer = 0;
    public float snappedMovementLerp = 1f;
    void ProcessMovementSnapped(Vector2 value, float delay) {
        var carts = Train.s.carts;

        cameraCenter.position = Vector3.Lerp(cameraCenter.position, carts[targetCart].position, snappedMovementLerp*Time.deltaTime); 

        if (snappedMoveTimer <= 0) {
            if (value.x < -0.1f || value.y < -0.1f) {
                targetCart += 1;
                snappedMoveTimer = delay;
                targetCart = Mathf.Clamp(targetCart, 0, carts.Count-1);
            }else if (value.x > 0.1f || value.y > 0.1f) {
                targetCart -= 1;
                snappedMoveTimer = delay;
                targetCart = Mathf.Clamp(targetCart, 0, carts.Count-1);
            }
        }

        if (value.sqrMagnitude < 0.1f) {
            snappedMoveTimer = 0;
        }

        snappedMoveTimer -= Time.deltaTime;
    }

    void SnapToNearestCart() {
        var carts = Train.s.carts;
        if (!snappedToTrainLastFrame) {
            targetCart = -1;
            var minDist = float.MaxValue;
            
            for (int i = 0; i < carts.Count; i++) {
                var dist = Vector3.Distance(cameraCenter.position, carts[i].position);

                if (dist < minDist) {
                    targetCart = i;
                    minDist = dist;
                }
            }
        }
    }
    
    

    public void FlipCamera(InputAction.CallbackContext info) {
        isRight = !isRight;
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
    private bool rotLerping = true;

    public void ProcessDirectControl(Vector2 stickInput) {
        var realInput = stickInput*gamepadSensitivity + Mouse.current.delta.ReadValue() * mouseSensitivity;
        realInput *= Time.deltaTime;

        rotTarget += realInput;

        Quaternion xQuaternion = Quaternion.AngleAxis (rotTarget.x, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis (rotTarget.y, -Vector3.right);
        
        var targetPos = directControlTransform.position;
        var targetRot = directControlTransform.rotation * xQuaternion * yQuaternion;
        cameraLerpDummy.transform.position = Vector3.Lerp(cameraLerpDummy.transform.position, targetPos, posLerpSpeed * Time.deltaTime);

        if (rotLerping) {
            cameraLerpDummy.transform.rotation = Quaternion.Lerp(cameraLerpDummy.transform.rotation, targetRot, rotLerpSpeed * Time.deltaTime);
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
}
