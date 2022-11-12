using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GunModule : MonoBehaviour, IComponentWithTarget, IActiveDuringCombat {
    [System.Serializable]
    public class TransformWithActivation {
        public Transform transform;
    }
    
    public TransformWithActivation[] rotateTransforms;
    public TransformWithActivation[] barrelEndTransforms;
    public float projectileSpawnOffset = 0.2f;
    

    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;


    public Transform target;


    public bool mortarRotation = false;
    public float fireDelay = 2f;
    public int fireBarrageCount = 5;
    public float fireBarrageDelay = 0.1f;
    public float projectileDamage = 2f;

    public float rotateSpeed = 10f;

    public bool gunActive = true;
    public bool CanShoot = false;
    public bool hasAmmo = true;

    public bool isPlayer = false;

    public Transform rangeOrigin;

    public UnityEvent barrageShot;

    public bool canPenetrateArmor = false;

    /*public float ammoUsePerShot = 0;
    public float scrapUsePerShot = 0;*/
    public float fuelUsePerShot = 0;
    public float steamUsePerShot = 0;
    
    private void Update() {
        if (gunActive) {
            if (target != null) {
                // Look at target
                LookAtLocation(target.position);

                if (rotateTransforms.Length == 0) {
                    CanShoot = true;
                }
            } else {

                // look at center of targeting area
                LookAtLocation(GetRangeOrigin().position + GetRangeOrigin().forward * 5);
            }
        }
    }

    public void LookAtLocation(Vector3 location) {
        for (int i = 0; i < rotateTransforms.Length; i++) {
            var rotateTransform = rotateTransforms[i].transform;
            var lookAxis = location - rotateTransform.position;
            if (mortarRotation)
                lookAxis.y = 0;
            var lookRotation = Quaternion.LookRotation(lookAxis, Vector3.up);
            //print(lookRotation);
            rotateTransform.rotation = Quaternion.Lerp(rotateTransform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
            if (Quaternion.Angle(rotateTransform.rotation, lookRotation) < 5) {
                CanShoot = true;
            }
        }
    }


    private Cart myCart = null;
    private void Start() {
        myCart = GetComponentInParent<Cart>();
    }

    float GetAttackSpeedMultiplier() {
        if (myCart != null) {
            return 1f / myCart.attackSpeedModifier;
        } else {
            return 1f;
        }
    }
    
    float GetDamageMultiplier() {
        if (myCart != null) {
            return 1f * myCart.damageModifier;
        } else {
            return 1f;
        }
    }


    private IEnumerator ActiveShootCycle;
    private bool isShooting = false;
    IEnumerator ShootCycle() {
        while (true) {
            yield return new WaitForSeconds(fireDelay * GetAttackSpeedMultiplier());
            while (!CanShoot || !hasAmmo) {
                yield return null;
            }
            if (isShooting) {
                StartCoroutine(ShootBarrage());
            } else {
                break;
            }
        }
    }

    IEnumerator ShootBarrage(bool isFree = false, GenericCallback shotCallback = null, GenericCallback onHitCallback = null) {
        for (int i = 0; i < fireBarrageCount; i++) {
            if (AreThereEnoughMaterialsToShoot() || isFree) {
                var barrelEnd = GetShootTransform().transform;
                var position = barrelEnd.position;
                var rotation = barrelEnd.rotation;
                var bullet = Instantiate(bulletPrefab, position + barrelEnd.forward * projectileSpawnOffset, rotation);
                var muzzleFlash = Instantiate(muzzleFlashPrefab, position, rotation);
                var projectile = bullet.GetComponent<Projectile>();
                projectile.myOriginObject = this.gameObject;
                projectile.damage = projectileDamage*GetDamageMultiplier();
                projectile.isTargetSeeking = true;
                projectile.canPenetrateArmor = canPenetrateArmor;
                
                projectile.isPlayerBullet = isPlayer;
                projectile.source = this;

                projectile.onHitCallback = onHitCallback;

                if(myCart != null)
                    LogShotData(projectileDamage*GetDamageMultiplier());

                if (isPlayer && !isFree) {
                    /*MoneyController.s.SubtractAmmo(ammoUsePerShot);
                    MoneyController.s.SubtractScraps(scrapUsePerShot);*/
                    MoneyController.s.ModifyResource(ResourceTypes.fuel, -fuelUsePerShot);
                    SpeedController.s.UseSteam(steamUsePerShot);
                }
                
                shotCallback?.Invoke();
            }
            yield return new WaitForSeconds(fireBarrageDelay);
        }
        
        if(!isFree)
            barrageShot?.Invoke();
    }
    public void ShootBarrageFree(GenericCallback shotCallback, GenericCallback onHitCallback) {
        StartCoroutine(ShootBarrage(true, shotCallback, onHitCallback));
    }

    bool AreThereEnoughMaterialsToShoot() {
        var areThereEnough = true;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        /*areThereEnough = areThereEnough && MoneyController.s.ammo >= ammoUsePerShot;
        areThereEnough = areThereEnough && MoneyController.s.scraps >= scrapUsePerShot;*/
        areThereEnough = areThereEnough && SpeedController.s.fuel >= fuelUsePerShot;
        
        return areThereEnough;
    }

    void LogShotData(float damage) {
        var currentLevelStats = PlayerBuildingController.s.currentLevelStats;
        var buildingName = GetComponent<TrainBuilding>().uniqueName;
        if (currentLevelStats.TryGetValue(buildingName, out PlayerBuildingController.BuildingData data)) {
            data.damageData += damage;
        } else {
            var toAdd = new PlayerBuildingController.BuildingData();
            toAdd.uniqueName = buildingName;
            toAdd.damageData += damage;
            currentLevelStats.Add(buildingName, toAdd);
        }
    }

    private int lastIndex = -1;
    public TransformWithActivation GetShootTransform() {
        List<TransformWithActivation> activeTransforms = new List<TransformWithActivation>();

        for (int i = 0; i < barrelEndTransforms.Length; i++) {
            if (barrelEndTransforms[i].transform.gameObject.activeInHierarchy) {
                activeTransforms.Add(barrelEndTransforms[i]);
            }
        }

        if (activeTransforms.Count == 0) {
            activeTransforms.Add(barrelEndTransforms[0]);
        }

        lastIndex++;
        lastIndex = lastIndex % activeTransforms.Count;
        return activeTransforms[lastIndex];
    }
    

    public void SetTarget(Transform target) {
        this.target = target;
        StartShooting();
    }

    public void DeactivateGun() {
        gunActive = false;
        StopShooting();
    }

    public void ActivateGun() {
        gunActive = true;
        StartShooting();
    }

    void StopShooting() {
        if (isShooting) {
            if(ActiveShootCycle != null)
                StopCoroutine(ActiveShootCycle);
            ActiveShootCycle = null;
            isShooting = false;
        }
    }

    void StartShooting() {
        if (gunActive) {
            if (!isShooting) {
                StopAllCoroutines();

                ActiveShootCycle = ShootCycle();
                StartCoroutine(ActiveShootCycle);
                isShooting = true;
            }
        }
    }

    public void UnsetTarget() {
        this.target = null;
        StopShooting();
    }

    public Transform GetRangeOrigin() {
        if (rangeOrigin != null) {
            return rangeOrigin;
        } else {
            return transform;
        }
    }

    public Transform GetActiveTarget() {
        return target;
    }

    public int GetDamage() {
        return (int)projectileDamage;
    }
    
    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
