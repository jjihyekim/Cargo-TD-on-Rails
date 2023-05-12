using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonAnimator : MonoBehaviour {


    public float goBackDelta = 0f;
    public float goBackStartDelta = -0.02f;
    public float goBackDeltaAcceleration = 0.15f;
    public Transform frontBit;
    public Transform frontBitRetractedPos;
    private Vector3 basePos;
    void Start()
    {
        GetComponentInParent<GunModule>().onBulletFiredEvent.AddListener(bulletFired);
        basePos = frontBit.transform.localPosition;
    }

    private bool isMoving = false;
    void bulletFired() {
        frontBit.transform.localPosition = frontBitRetractedPos.transform.localPosition;
        goBackDelta = goBackStartDelta;
        isMoving = true;
    }

    // Update is called once per frame
    void Update() {
        if (isMoving) {
            frontBit.transform.localPosition += Vector3.forward * goBackDelta;
            goBackDelta += goBackDeltaAcceleration * Time.deltaTime;


            if (frontBit.transform.localPosition.z > basePos.z) {
                frontBit.transform.localPosition = basePos;
                isMoving = false;
            }
        }
    }
}
