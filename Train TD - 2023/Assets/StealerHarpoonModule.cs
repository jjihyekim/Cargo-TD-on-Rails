using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealerHarpoonModule : MonoBehaviour {
    
    public float range = 2;
    public GameObject harpoon;
    public Transform harpoonSpawnLocation;
    public Transform ropeEndPoint;

    public Transform harpoonRotatePoint;

    public float cargoStealInitialDelay = 5f;
    public float cargoStealInterval = 5f;
    public int cargoStealPerInterval = 10;

    public int stealBeforeLeaving = 50;
    public int currentlyStolen = 0;

    public bool isLeaving = false;
    private float curTimer;

    public ModuleStorage target;
    //public rope
    public void SetTarget(ModuleStorage _target) {
        if(isLeaving)
            return;

        target = _target;
        curTimer = cargoStealInitialDelay;

        DisconnectHarpoon();
    }

    void DisconnectHarpoon() {
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

            harpoonReAttachTimer = 2f;
        }
    }

    public bool ropeLerpInProgress = false;

    public GameObject harpoonShootEffect;
    public GameObject harpoonDisengageEffect;
    public GameObject harpoonStealEffect;
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


        while (Vector3.Distance(harpoon.transform.position, harpoonEngagePoint.transform.position) > 0.01f) {
            harpoon.transform.position = Vector3.MoveTowards(harpoon.transform.position, harpoonEngagePoint.transform.position, 6 * Time.deltaTime);
            yield return null;
        }

        harpoon.transform.position = harpoonEngagePoint.transform.position;
        harpoon.transform.SetParent(harpoonEngagePoint.transform);

        harpoonLerpInProgress = false;
        harpoonEngaged = true;
    }


    public bool harpoonEngaged = false;
    public bool canShoot;
    private float harpoonReAttachTimer;
    private void Update() {
        if(isLeaving)
            return;

        if (target != null){
            var lookAxis = target.transform.position - harpoonRotatePoint.position;
            var lookRotation = Quaternion.LookRotation(lookAxis, Vector3.up);
            harpoonRotatePoint.rotation = Quaternion.Lerp(harpoonRotatePoint.rotation, lookRotation, 20 * Time.deltaTime);
            canShoot = Quaternion.Angle(harpoonRotatePoint.rotation, lookRotation) < 5;

            if (harpoonEngaged) {
                
                if (Vector3.Distance(transform.position, target.transform.position) > range) {
                    DisconnectHarpoon();
                }
                
                curTimer -= Time.deltaTime;

                if (curTimer <= 0) {
                    var enemyStorage = GetComponentInParent<EnemyHealth>();

                    var storageType = target.myType;

                    if (MoneyController.s.HasResource(storageType, cargoStealPerInterval)) {
                        
                        MoneyController.s.ModifyResource(storageType, -cargoStealPerInterval);
                        switch (storageType) {
                            case ResourceTypes.scraps:
                                enemyStorage.scrapReward += cargoStealPerInterval;
                                break;
                        }
                        
                        Instantiate(harpoonStealEffect, harpoon.transform.position, harpoon.transform.rotation);
                    
                        LevelReferences.s.SpawnResourceAtLocation(storageType, cargoStealPerInterval, target.transform.position, true, transform);

                        currentlyStolen += cargoStealPerInterval;

                        if (currentlyStolen >= stealBeforeLeaving) {
                            SetTarget(null);
                            isLeaving = true;
                            Invoke(nameof(Leave), 1f);
                        }
                    }

                    curTimer += cargoStealInterval;


                    
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

    void Leave() {
        GetComponentInParent<EnemyWave>().Leave(true);
    }
}
