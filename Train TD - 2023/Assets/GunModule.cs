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


    public bool isGigaGatling = false;

    public float fireDelayReductionPerGatlingAmount = 2f;
    public int gatlingAmount;
    public int maxGatlingAmount;
    
    public TransformWithActivation[] rotateTransforms;
    public TransformWithActivation[] barrelEndTransforms;
    public float projectileSpawnOffset = 0.2f;
    

    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;


    public Transform target;


    public bool mortarRotation = false;
    public float fireDelay = 2f; // dont use this

    public float GetFireDelay() {
        if (isGigaGatling) { 
            return (fireDelay-(Mathf.Pow(gatlingAmount, 1/2f)*fireDelayReductionPerGatlingAmount)) * GetAttackSpeedMultiplier();
            
        } else {
            return fireDelay * GetAttackSpeedMultiplier();
        }
    }

    public int fireBarrageCount = 5;
    public float fireBarrageDelay = 0.1f;// dont use this
    public float fireRateMultiplier = 1f;
    public float GetFireBarrageDelay() { return fireBarrageDelay * GetAttackSpeedMultiplier();}
    public float projectileDamage = 2f; // dont use this
    public float damageMultiplier = 1f;
    public bool dontGetAffectByMultipliers = false;

    public bool isHeal = false;

    public float GetDamage() {
        if (dontGetAffectByMultipliers) {
            return projectileDamage;
        } else {
            return projectileDamage * GetDamageMultiplier();
        }
    }


    public float rotateSpeed = 10f;

    public bool gunActive = true;
    public bool IsBarrelPointingCorrectly = false;
    public bool hasAmmo = true;

    public bool isPlayer = false;

    public Transform rangeOrigin;

    public UnityEvent barrageShot;

    public bool canPenetrateArmor = false;

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
    private float gunShakeMagnitude = 0.04f;
    private Vector3 gunShakeRotation = new Vector3(-2,0,0);

    public bool beingDirectControlled = false;


    public float boostDamageOnUpgrade = 0.5f;

    public void ResetState(int level) {
        damageMultiplier = 1 + (boostDamageOnUpgrade*level);
        fireRateMultiplier = 1;
    }
    
    private void Update() {
        if (gunActive) {
            if (target != null) {
                // Look at target
                if (rotateTransforms.Length == 0) {
                    IsBarrelPointingCorrectly = true;
                } else {
                    LookAtLocation(target.position);
                }
            } else {
                // look at center of targeting area
                LookAtLocation(GetRangeOrigin().position + GetRangeOrigin().forward * 5);

                IsBarrelPointingCorrectly = false;
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
                    IsBarrelPointingCorrectly = true;
                }else {
                    IsBarrelPointingCorrectly = false;
                }
            }
        //}
    }


    private void Start() {
        for (int i = 0; i < rotateTransforms.Length; i++) {
            var curRotate = rotateTransforms[i].transform;
            var anchor = new GameObject("Turret Rotate Anchor");
            anchor.transform.SetParent(curRotate.parent);
            anchor.transform.position = curRotate.position;
            curRotate.SetParent(anchor.transform);
        }
    }

    float GetAttackSpeedMultiplier() {
        var boost = 1f/fireRateMultiplier;

        if (isPlayer) {
            boost /= TweakablesMaster.s.myTweakables.playerFirerateBoost;
        } else {
            boost /= TweakablesMaster.s.myTweakables.enemyFirerateBoost;
        }

        return boost;
    }

    float GetDamageMultiplier() {
        var dmgMul = damageMultiplier;
        
        if (isPlayer) {
            dmgMul *= TweakablesMaster.s.myTweakables.playerDamageMultiplier;
        } else {
            dmgMul *= TweakablesMaster.s.myTweakables.enemyDamageMutliplier + WorldDifficultyController.s.currentDamageIncrease;
        }

        return dmgMul;
    }


    private IEnumerator ActiveShootCycle;
    private bool isShooting = false;
    public float waitTimer;
    IEnumerator ShootCycle() {
        while (true) {
            while (!IsBarrelPointingCorrectly || !hasAmmo || waitTimer > 0) {
                waitTimer -= Time.deltaTime;
                yield return null;
            }
            
            if (isShooting) {
                StartCoroutine(_ShootBarrage());
            } else {
                break;
            }

            waitTimer = GetFireDelay();
        }
    }

    private float stopShootingTimer = 0f;
    void StopShootingFindingHelperThingy() {
        stopShootingEvent?.Invoke();
        isWarmedUp = false;
    }
    
    IEnumerator _ShootBarrage(bool isFree = false, GenericCallback shotCallback = null, GenericCallback onHitCallback = null) {
        stopShootingTimer = GetFireDelay()+0.05f;
        if (!isWarmedUp) {
            isWarmedUp = true;
            startWarmUpEvent?.Invoke();
            
            if (needWarmUp) {
                yield break;
            }
        }
        
        
        if(!isFree)
            barrageShot?.Invoke();

        for (int i = 0; i < fireBarrageCount; i++) {
            //if (!isPlayer || AreThereEnoughMaterialsToShoot() || isFree) {
                var barrelEnd = GetShootTransform().transform;
                var position = barrelEnd.position;
                var rotation = barrelEnd.rotation;
                var bullet = Instantiate(bulletPrefab, position + barrelEnd.forward * projectileSpawnOffset, rotation);
                var muzzleFlash = Instantiate(muzzleFlashPrefab, position, rotation);
                var projectile = bullet.GetComponent<Projectile>();
                projectile.myOriginObject = this.transform.root.gameObject;
                projectile.damage = GetDamage();
                projectile.target = target;
                projectile.isHeal = isHeal;
                //projectile.isTargetSeeking = true;
                projectile.canPenetrateArmor = canPenetrateArmor;
                if (beingDirectControlled) {
                    projectile.speed *= 2;
                    projectile.acceleration *= 2;
                }

                projectile.SetIsPlayer(isPlayer);
                projectile.source = this;

                projectile.onHitCallback = onHitCallback;

                //if(myCart != null)
                if (isPlayer)
                    LogShotData(GetDamage());
                if (isPlayer && !isFree) {
                    SpeedController.s.UseSteam(steamUsePerShot*TweakablesMaster.s.myTweakables.gunSteamUseMultiplier);
                }
                
                shotCallback?.Invoke();
                onBulletFiredEvent?.Invoke();
                if(gunShakeOnShoot)
                    StartCoroutine(ShakeGun());

                if (isGigaGatling) {
                    gatlingAmount += 1;
                    gatlingAmount = Mathf.Clamp(gatlingAmount, 0, maxGatlingAmount);
                }
            //}
            yield return new WaitForSeconds(GetFireBarrageDelay());
        }
    }

    private bool stopUpdateRotation = false;

    public IEnumerator ShakeGun() {
        yield return null;
        
        var range = Mathf.Clamp01(GetDamage() / 10f) + Mathf.Clamp01(GetDamage() / 10f);
        range /= 2f;

        var defaultPositions = new List<Vector3>();

        var realMagnitude = gunShakeMagnitude;
        if (beingDirectControlled) {
            realMagnitude *= CameraShakeController.s.overallShakeAmount;
        }
        
        //stopUpdateRotation = true;
        for (int i = 0; i < rotateTransforms.Length; i++) {
            var rotateTransform = rotateTransforms[i].transform;
            defaultPositions.Add(rotateTransform.localPosition);
            rotateTransform.localPosition = Random.insideUnitSphere * realMagnitude * range + (-rotateTransform.forward * realMagnitude * range * 2);
            rotateTransform.Rotate(gunShakeRotation);
        }

        yield return null;
        //stopUpdateRotation = false;
        for (int i = 0; i < rotateTransforms.Length; i++) {
            var rotateTransform = rotateTransforms[i].transform;
            rotateTransform.localPosition= defaultPositions[i];
        }
    }

    [Button]
    public void ShootBarrageDebug() {
        StartCoroutine(_ShootBarrage(true));
    }
    [Button]
    public void ShootBarrageContinuousDebug() {
        StartCoroutine(ShootCycle());
    }
    
    public void ShootBarrage(bool isFree, GenericCallback shotCallback, GenericCallback onHitCallback) {
        StartCoroutine(_ShootBarrage(isFree, shotCallback, onHitCallback));
    }


    void LogShotData(float damage) {
        /*var currentLevelStats = PlayerBuildingController.s.currentLevelStats;
        var buildingName = GetComponent<TrainBuilding>().uniqueName;
        if (currentLevelStats.TryGetValue(buildingName, out PlayerBuildingController.BuildingData data)) {
            data.damageData += damage;
        } else {
            var toAdd = new PlayerBuildingController.BuildingData();
            toAdd.uniqueName = buildingName;
            toAdd.damageData += damage;
            currentLevelStats.Add(buildingName, toAdd);
        }*/
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
                isShooting = true;
                StartCoroutine(ActiveShootCycle);
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

    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
