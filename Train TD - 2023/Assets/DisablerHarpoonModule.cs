using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablerHarpoonModule : MonoBehaviour {
    
    public float range = 2;
    public GameObject harpoon;
    public Transform harpoonSpawnLocation;
    public Transform ropeEndPoint;

    public Transform harpoonRotatePoint;

    public Cart target;

    public LineRenderer _lineRenderer;

    public GameObject sparks;

    public bool doDisable = true;

    public void UpdateColor(float percent) {
        percent = Mathf.Clamp01(percent);
        var gradient = new Gradient();

        
        var colors = new GradientColorKey[2];
        colors[0] = new GradientColorKey(Color.black, percent);
        colors[1] = new GradientColorKey(Color.white, percent+0.05f);

        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 1.0f);
        alphas[1] = new GradientAlphaKey(0.0f, 1.0f);

        gradient.SetKeys(colors, alphas);

        _lineRenderer.colorGradient = gradient;
        
        sparks.gameObject.SetActive(harpoonEngaged);

        var pos = _lineRenderer.positionCount * percent;
        var prevPos = _lineRenderer.GetPosition(Mathf.Clamp(Mathf.FloorToInt(pos),0,_lineRenderer.positionCount-1));
        var nextPos = _lineRenderer.GetPosition(Mathf.Clamp(Mathf.CeilToInt(pos),0,_lineRenderer.positionCount-1));
        
        sparks.transform.position = Vector3.Lerp(prevPos,nextPos, pos%1);
    }
    
    //public rope
    public void SetTarget(Cart _target) {
        DisconnectHarpoon(target);
        target = _target;
    }

    void DisconnectHarpoon(Cart disconnectingTarget) {
        if (harpoonEngaged) {
            if (!harpoonLerpInProgress) {
                Instantiate(harpoonDisengageEffect, harpoon.transform.position, harpoon.transform.rotation);
                
                harpoon = Instantiate(harpoon, harpoonSpawnLocation.position, harpoonSpawnLocation.rotation);
                
                harpoon.transform.SetParent(harpoonSpawnLocation);
                ropeLerpInProgress = true;
                StartCoroutine(RopeLerp());
            } else {
                StopAllCoroutines();
                harpoon.transform.position = harpoonSpawnLocation.position;
                harpoon.transform.rotation = harpoonSpawnLocation.rotation;
                
                var ropeTarget = harpoon.transform.GetChild(0);
                ropeEndPoint.transform.position = ropeTarget.transform.position;
                ropeEndPoint.SetParent(ropeTarget);
                ropeLerpInProgress = false;
                harpoonLerpInProgress = false;
            }

            harpoonEngaged = false;
            SetIfCartIsActive(disconnectingTarget,true);

            harpoonReAttachTimer = 2f;
        }
    }

    public bool ropeLerpInProgress = false;

    public GameObject harpoonShootEffect;
    public GameObject harpoonDisengageEffect;
    public GameObject harpoonActiveEffect;
    IEnumerator RopeLerp() {
        ropeEndPoint.SetParent(null);
        var ropeTarget = harpoon.transform.GetChild(0);
        while (ropeEndPoint.transform.position.y < 1) {
            ropeEndPoint.transform.position += Vector3.up * Time.deltaTime * 3;
            yield return null;
        }
        
        while (Vector3.Distance(ropeEndPoint.transform.position, ropeTarget.transform.position) > 0.01f) {
            ropeEndPoint.transform.position = Vector3.MoveTowards(ropeEndPoint.transform.position, ropeTarget.transform.position, 3 * Time.deltaTime);
            yield return null;
        }

        ropeEndPoint.transform.position = ropeTarget.transform.position;
        ropeEndPoint.SetParent(ropeTarget);

        yield return new WaitForSeconds(2);
        ropeLerpInProgress = false;
    }
    
    public bool harpoonLerpInProgress = false;
    IEnumerator HarpoonLerp() {
        var harpoonEngagePoint = new GameObject("HarpoonEngagePoint");
        var targetCollider = target.GetComponentInChildren<BoxCollider>();
        harpoonEngagePoint.transform.position = targetCollider.ClosestPoint(transform.position);
        harpoonEngagePoint.transform.SetParent(target.transform);


        while (Vector3.Distance(harpoon.transform.position, harpoonEngagePoint.transform.position) > 0.05f) {
            harpoon.transform.position = Vector3.MoveTowards(harpoon.transform.position, harpoonEngagePoint.transform.position, 6 * Time.deltaTime);
            yield return null;
        }

        harpoon.transform.position = harpoonEngagePoint.transform.position;
        harpoon.transform.SetParent(harpoonEngagePoint.transform);

        harpoonLerpInProgress = false;
        harpoonEngaged = true;
        SetIfCartIsActive(target,false);
    }


    public bool harpoonEngaged = false;
    public bool canShoot;
    private float harpoonReAttachTimer;
    private float activeEffectTimer;
    private void Update() {
        if (target != null){
            var lookAxis = target.transform.position - harpoonRotatePoint.position;
            var lookRotation = Quaternion.LookRotation(lookAxis, Vector3.up);
            harpoonRotatePoint.rotation = Quaternion.Lerp(harpoonRotatePoint.rotation, lookRotation, 20 * Time.deltaTime);
            canShoot = Quaternion.Angle(harpoonRotatePoint.rotation, lookRotation) < 5;

            if (harpoonEngaged) {
                if (Vector3.Distance(transform.position, target.transform.position) > range) {
                    DisconnectHarpoon(target);
                }

                if (activeEffectTimer <= 0) {
                    Instantiate(harpoonActiveEffect, harpoon.transform.position, harpoon.transform.rotation);
                    activeEffectTimer += 1f;
                } else {
                    activeEffectTimer -= Time.deltaTime;
                }
            } else {
                if (Vector3.Distance(transform.position, target.transform.position) < range) {
                    harpoonReAttachTimer -= Time.deltaTime;

                    if (harpoonReAttachTimer <= 0) {
                        if (!ropeLerpInProgress && !harpoonLerpInProgress && canShoot) {
                            harpoonLerpInProgress = true;
                            Instantiate(harpoonShootEffect, harpoon.transform.position, harpoon.transform.rotation);
                            StartCoroutine(HarpoonLerp());
                        }
                    }
                }
            }
        }
    }

    void SetIfCartIsActive(Cart cart, bool isActive) {
        if (doDisable) {
            if (cart != null) {
                if (isActive) {
                    cart.isBeingDisabled = false;
                    cart.SetDisabledState();
                } else {
                    cart.isBeingDisabled = true;
                    cart.SetDisabledState();
                }
            }
        }
    }

    private void OnDestroy() {
        SetIfCartIsActive(target, true);
    }
}
