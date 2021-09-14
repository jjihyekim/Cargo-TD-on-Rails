using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScroll : MonoBehaviour {

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

    public float scrollSpeed = 2f;
    public float wasdSpeed = 2f;
    public float zoomSpeed = 0.1f;

    public float posLerpSpeed = 1f;
    public float rotationAngle = 50;
    public float rotLerpSpeed = 20f;

    public InputActionProperty moveAction;
    public InputActionProperty rotateAction;
    public InputActionProperty zoomAction;


    public float currentZoom = 0;
    public Vector2 zoomLimit = new Vector2(-2, 2);
    protected void OnEnable()
    {
        moveAction.action.Enable();
        rotateAction.action.Enable();
        zoomAction.action.Enable();
        rotateAction.action.performed += FlipCamera;
    }


    protected void OnDisable()
    {
        moveAction.action.Disable();
        rotateAction.action.Disable();
        zoomAction.action.Disable();
        rotateAction.action.performed -= FlipCamera;
    }

    private GameObject cameraLerpDummy;
    private void Start() {
        cameraCenter.transform.rotation = Quaternion.Euler(0, isRight ? -rotationAngle : rotationAngle, 0);
        cameraLerpDummy = new GameObject();
        cameraLerpDummy.name = "Camera Lerp Dummy";
        cameraLerpDummy.transform.SetParent(cameraOffset);
        cameraLerpDummy.transform.position = cameraOffset.position;
        cameraLerpDummy.transform.rotation = cameraOffset.rotation;
        SetMainCamPos();
    }

    private void LateUpdate() {
        ProcessScreenCorners(Mouse.current.position.ReadValue());
        ProcessMovementInput(moveAction.action.ReadValue<Vector2>(), wasdSpeed);
        ProcessZoom(zoomAction.action.ReadValue<float>());

        var centerRotTarget = Quaternion.Euler(0, isRight ? -rotationAngle : rotationAngle, 0);
        
        cameraCenter.transform.rotation = Quaternion.Lerp(cameraCenter.transform.rotation, centerRotTarget, rotLerpSpeed * Time.deltaTime);

        var targetPos = cameraOffset.position + cameraOffset.forward * currentZoom;
        var targetRot = cameraOffset.rotation;
        cameraLerpDummy.transform.position = Vector3.Lerp(cameraLerpDummy.transform.position, targetPos, posLerpSpeed * Time.deltaTime);
        cameraLerpDummy.transform.rotation = Quaternion.Lerp(cameraLerpDummy.transform.rotation, targetRot, rotLerpSpeed * Time.deltaTime);
        SetMainCamPos();
    }

    void SetMainCamPos() {
        mainCamera.transform.position = cameraLerpDummy.transform.position;
        mainCamera.transform.rotation = cameraLerpDummy.transform.rotation;
    }

    void ProcessZoom(float value) {
        currentZoom += value * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, zoomLimit.x, zoomLimit.y);
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
        
        ProcessMovementInput(output, scrollSpeed);
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
}
