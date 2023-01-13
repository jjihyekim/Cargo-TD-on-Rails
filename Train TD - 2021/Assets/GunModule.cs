using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GunModule : MonoBehaviour, IComponentWithTarget, IActiveDuringCombat {

    public Sprite gunSprite;
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
    public float fireDelay = 2f; // dont use this
    public float GetFireDelay() { return fireDelay * GetAttackSpeedMultiplier();}
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

    public bool needWarmUp = false;
    private bool isWarmedUp = false;

    [HideInInspector]
    public UnityEvent startWarmUpEvent = new UnityEvent();
    [HideInInspector]
    public UnityEvent onBulletFiredEvent = new UnityEvent();
    [HideInInspector]
    public UnityEvent stopShootingEvent = new UnityEvent();

    public bool gunShakeOnShoot = true;
    private float gunShakeMagnitude = 0.004f;
    private Vector3 gunShakeRotation = new Vector3(-2,0,0);
    
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

        if (isWarmedUp) {
            stopShootingTimer -= Time.deltaTime;
            if (stopShootingTimer <= 0) {
                StopShootingFindingHelperThingy();
            }
        }
    }

    public void LookAtLocation(Vector3 location) {
        //if (!stopUpdateRotation) {
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
        //}
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
            yield return new WaitForSeconds(GetFireDelay());
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

    private float stopShootingTimer = 0f;
    void StopShootingFindingHelperThingy() {
        stopShootingEvent?.Invoke();
        isWarmedUp = false;
    }
    
    IEnumerator ShootBarrage(bool isFree = false, GenericCallback shotCallback = null, GenericCallback onHitCallback = null) {
        stopShootingTimer = GetFireDelay()+0.05f;
        if (!isWarmedUp) {
            isWarmedUp = true;
            startWarmUpEvent?.Invoke();
            
            if (needWarmUp) {
                yield break;
            }
        }

        for (int i = 0; i < fireBarrageCount; i++) {
            if (!isPlayer || AreThereEnoughMaterialsToShoot() || isFree) {
                var barrelEnd = GetShootTransform().transform;
                var position = barrelEnd.position;
                var rotation = barrelEnd.rotation;
                var bullet = Instantiate(bulletPrefab, position + barrelEnd.forward * projectileSpawnOffset, rotation);
                var muzzleFlash = Instantiate(muzzleFlashPrefab, position, rotation);
                var projectile = bullet.GetComponent<Projectile>();
                projectile.myOriginObject = this.gameObject;
                projectile.damage = projectileDamage*GetDamageMultiplier();
                //projectile.isTargetSeeking = true;
                projectile.canPenetrateArmor = canPenetrateArmor;
                
                projectile.isPlayerBullet = isPlayer;
                projectile.source = this;

                projectile.onHitCallback = onHitCallback;

                if(myCart != null)
                    LogShotData(projectileDamage*GetDamageMultiplier());

                if (isPlayer && !isFree) {
                    /*MoneyController.s.SubtractAmmo(ammoUsePerShot);
                    MoneyController.s.SubtractScraps(scrapUsePerShot);*/
                    if (fuelUsePerShot > 0) {
                        MoneyController.s.ModifyResource(ResourceTypes.fuel, -fuelUsePerShot);
                    }
                    SpeedController.s.UseSteam(steamUsePerShot);
                }
                
                shotCallback?.Invoke();
                onBulletFiredEvent?.Invoke();
                if(gunShakeOnShoot)
                    StartCoroutine(ShakeGun());
            }
            yield return new WaitForSeconds(fireBarrageDelay);
        }
        
        if(!isFree)
            barrageShot?.Invoke();
    }

    private bool stopUpdateRotation = false;

    private bool realPositionsSet = false;
    Vector3[] realPositions;
    void RegisterRotateTransformRealPositions() {
        realPositions = new Vector3[rotateTransforms.Length];
        for (int i = 0; i < rotateTransforms.Length; i++) {
            var rotateTransform = rotateTransforms[i].transform;
            realPositions[i] = rotateTransform.localPosition;
        }

        realPositionsSet = true;
    }
    public IEnumerator ShakeGun() {
        yield return null;
        //stopUpdateRotation = true;
        if (!realPositionsSet) {
            RegisterRotateTransformRealPositions();
        }
        for (int i = 0; i < rotateTransforms.Length; i++) {
            var rotateTransform = rotateTransforms[i].transform;
            rotateTransform.localPosition = Random.insideUnitSphere * gunShakeMagnitude;
            rotateTransform.Rotate(gunShakeRotation);
        }

        yield return null;
        //stopUpdateRotation = false;
        for (int i = 0; i < rotateTransforms.Length; i++) {
            var rotateTransform = rotateTransforms[i].transform;
            rotateTransform.localPosition= realPositions[i];
        }
    }

    [Button]
    public void ShootBarrageDebug() {
        StartCoroutine(ShootBarrage(true));
    }
    [Button]
    public void ShootBarrageContinuousDebug() {
        StartCoroutine(ShootCycle());
    }
    
    public void ShootBarrageFree(GenericCallback shotCallback, GenericCallback onHitCallback) {
        StartCoroutine(ShootBarrage(true, shotCallback, onHitCallback));
    }

    bool AreThereEnoughMaterialsToShoot() {
        var areThereEnough = true;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        /*areThereEnough = areThereEnough && MoneyController.s.ammo >= ammoUsePerShot;
        areThereEnough = areThereEnough && MoneyController.s.scraps >= scrapUsePerShot;*/
        areThereEnough = areThereEnough && MoneyController.s.HasResource(ResourceTypes.fuel, fuelUsePerShot);
        
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
