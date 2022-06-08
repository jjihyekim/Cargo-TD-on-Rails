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

    public bool canScroll = true;
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

    private GameObject cameraLerpDummy;
    private void Start() {
#if UNITY_EDITOR
        edgeScrollMoveSpeed = 0; // we dont want edge scroll in the editor
#endif
        cameraCenter.transform.rotation = Quaternion.Euler(0, isRight ? -rotationAngleTarget : rotationAngleTarget, 0);
        cameraLerpDummy = new GameObject();
        cameraLerpDummy.name = "Camera Lerp Dummy";
        cameraLerpDummy.transform.SetParent(cameraOffset);
        cameraLerpDummy.transform.position = cameraOffset.position;
        cameraLerpDummy.transform.rotation = cameraOffset.rotation;
        SetMainCamPos();
    }

    private void LateUpdate() {
        var mousePos = Mouse.current.position.ReadValue();
        ProcessScreenCorners(mousePos);
        ProcessMovementInput(moveAction.action.ReadValue<Vector2>(), wasdSpeed);
        if(canScroll)
            ProcessZoom(zoomAction.action.ReadValue<float>());
        ProcessMiddleMouseRotation(rotateCameraAction.action.ReadValue<float>(), mousePos);
        LerpCameraTarget();
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
        mainCamera.transform.position = cameraLerpDummy.transform.position;
        mainCamera.transform.rotation = cameraLerpDummy.transform.rotation;
    }

    void ProcessZoom(float value) {
        if (canZoom) {
            currentZoom += value * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, zoomLimit.x, zoomLimit.y);
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

    public void ProcessMovementInput(Vector2 value, float multiplier) {
        var delta = new Vector3(value.x, 0, value.y);
        
        var transformed = cameraOffsetFlat.TransformDirection(delta);
        

        cameraCenter.position += transformed * multiplier * Time.deltaTime;

        if (cameraCenter.transform.position.x < cameraCornerBottomLeft.position.x) {
            cameraCenter.position = new Vector3(cameraCornerBottomLeft.position.x, cameraCenter.position.y, cameraCenter.position.z);
        }else if (cameraCenter.position.x > cameraCornerTopRight.position.x) {
            cameraCenter.position = new Vector3(cameraCornerTopRight.position.x, cameraCenter.position.y, cameraCenter.position.z);
        }
        
        if (cameraCenter.position.z < cameraCornerBottomLeft.position.z) {
            cameraCenter.position = new Vector3(cameraCenter.position.x, cameraCenter.position.y, cameraCornerBottomLeft.position.z);
        }else if (cameraCenter.position.z > cameraCornerTopRight.position.z) {
            cameraCenter.position = new Vector3(cameraCenter.position.x, cameraCenter.position.y, cameraCornerTopRight.position.z);
        }
    }

    public void FlipCamera(InputAction.CallbackContext info) {
        isRight = !isRight;
    }

    
    public void ToggleCameraEdgeScroll() {
        canScroll = !canScroll;
    }
}
