using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallistaAnimator : MonoBehaviour {

    public Transform leftRotate;
    public Transform rightRotate;

    public float activeRotate;
    public float unActiveRotate;

    public LineRenderer leftLine;
    public LineRenderer rightLine;

    public Transform hook;
    public Transform hookUsed;
    public Transform hookCharged;
    public GameObject bullet;

    private GunModule myGun;

    public float currentCharge;
    public float chargeSpeed;

    private void Start() {
        myGun = GetComponentInParent<GunModule>();
        currentCharge = 1;
        myGun.onBulletFiredEvent.AddListener(OnShoot);
    }

    void OnShoot() {
        chargeSpeed = 1f / (myGun.GetFireDelay());
        currentCharge = 0;
    }

    private const float fullCharge = 0.9f;
    void Update() {
        var percent = Mathf.Clamp(currentCharge, 0, fullCharge) / fullCharge;

        bullet.SetActive(currentCharge > fullCharge);
        
        hook.transform.position = Vector3.Lerp(hookUsed.position, hookCharged.position, percent);
        
        leftRotate.transform.localRotation = Quaternion.Euler(0,Mathf.Lerp(unActiveRotate, activeRotate, percent), 0);
        rightRotate.transform.localRotation = Quaternion.Euler(0,-Mathf.Lerp(unActiveRotate, activeRotate, percent), 0);
        
        leftLine.SetPosition(1, leftLine.transform.InverseTransformPoint(hook.position));
        rightLine.SetPosition(1, rightLine.transform.InverseTransformPoint(hook.position));
        currentCharge += Time.deltaTime * chargeSpeed;
    }
}
