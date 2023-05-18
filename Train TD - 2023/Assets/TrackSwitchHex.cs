using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackSwitchHex : MonoBehaviour {


    public Transform topRotationHexParentPos;
    public Transform bottomRotationHexParentPos;

    public Transform attachedHex;

    private float multiplier = 0.6865f; // I have no idea where this number comes from but it works.

    public bool isGoingLeft = false;

    public GameObject directionArrow;

    public float GetSwitchDistance() {
        return transform.position.z + SpeedController.s.currentDistance;
    }

    public void SetUp() {
        PathSelectorController.s.RegisterTrackSwitchHex(this);
    }

    private float prevAngle = 0;
    void Update() {
        var gridSizeX = HexGrid.s.gridSize.x;
        var zPos = transform.position.z/ gridSizeX;
        zPos = Mathf.Clamp(zPos, -0.5f, 0.5f);
        
        directionArrow.transform.rotation = Quaternion.Euler(0, !isGoingLeft? 180: 0,0);
        if (!isGoingLeft) {
            directionArrow.GetComponent<MeshRenderer>().material.color = LevelReferences.s.leftColor;
        } else {
            directionArrow.GetComponent<MeshRenderer>().material.color = LevelReferences.s.rightColor;
        }

        if (zPos < 0.5f) {
            var startPos = transform.position.z;
            Train.s.UpdateTrainCartsBasedOnRotation(startPos, startPos + gridSizeX/2f, gridSizeX*multiplier, gridSizeX/2f, isGoingLeft);
        }
        
        if (zPos < 0) {
            var angle = 90 * zPos;
            transform.rotation = Quaternion.Euler(0,angle * (isGoingLeft ? -1 : 1), 0);
            var pos = transform.position;

            pos.x = 1-Mathf.Cos(angle * Mathf.Deg2Rad);
            //print($"{pos.x}");
            pos.x *= (gridSizeX*multiplier *(isGoingLeft ? -1 : 1));
            

            transform.position = pos;
            
            CameraController.s.ManualRotateDirectControl(angle-prevAngle);
            prevAngle = angle;
        }

        if (zPos <= -0.5f) {
            enabled = false;
        }
    }


    public void AttachHex(Transform toAttach, Transform toAttach2) {
        attachedHex = toAttach;
        
        toAttach.SetParent(bottomRotationHexParentPos);
        toAttach.localPosition = Vector3.forward * HexGrid.s.gridOffset * (bottomRotationHexParentPos.childCount - 1);
        toAttach.localRotation = Quaternion.identity;

        toAttach2.SetParent(topRotationHexParentPos);
        toAttach2.localPosition = Vector3.forward * HexGrid.s.gridOffset * (bottomRotationHexParentPos.childCount - 1);
        toAttach2.localRotation = Quaternion.identity;
    }
}
