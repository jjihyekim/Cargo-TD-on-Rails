using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModuleHealth : MonoBehaviour, IHealth, IActiveDuringCombat, IActiveDuringShopping {

    public static bool isImmune = false;

    public float maxHealth = 50;
    public float currentHealth = 50;

    public float damageReductionMultiplier = 1f;


    [ReadOnly]
    public MiniGUI_HealthBar healthBar;
    
    public GameObject explodePrefab;
    public bool isDead = false;

    public static int buildingsBuild;
    public static int buildingsDestroyed;

    public bool damageNearCartsOnDeath = false;
    public bool selfDamage = false;
    [ShowIf("selfDamage")] 
    private float selfDamageTimer;
    public int[] selfDamageAmounts = new[] { 20, 10 };

    private Cart myBuilding;
    public bool isCartHp = false;

    private GameObject activeHPCriticalIndicatorEffects;
    private enum HPLowStates {
        full, low, critical, destroyed
    }

    private HPLowStates hpState;

    public bool invincibleTutorial = false;

    public PhysicalHealthBar myBar;

    
    [Button]
    public void DealDamage(float damage) {
        Assert.IsTrue(damage > 0);
        if(isImmune)
            return;

        myBuilding = GetComponent<Cart>();
        if (!isDead && (myBuilding == null || !myBuilding.isDestroyed)) {
            currentHealth -= damage;

            var hpPercent = currentHealth / maxHealth;
            SetBuildingShaderHealth(hpPercent);
            
            if(currentHealth <= 0) {
                var repairable = GetComponent<RepairableIfDestroyed>();
                if (repairable != null) {
                    GetDestroyed();
                } else {
                    if(!invincibleTutorial)
                        Die();
                    else 
                        currentHealth = 1;
                }
            }

            UpdateHPCriticalIndicators();

            if (currentHealth < 0) {
                currentHealth = 0;
            }
            
            myBar.UpdateHealth(currentHealth/maxHealth);
        }
    }

    void UpdateHPCriticalIndicators() {
        var hpPercent = currentHealth / maxHealth;
        if(isDead)
            return;
        myBuilding = GetComponent<Cart>();
        if (myBuilding == null) { // carts have module health but not trainbuilding
            return;
        }

        if (myBuilding.isDestroyed) {
            if (hpState != HPLowStates.destroyed) {
                if(activeHPCriticalIndicatorEffects != null)
                    activeHPCriticalIndicatorEffects.GetComponent<SmartDestroy>().Engage();
                activeHPCriticalIndicatorEffects = Instantiate(LevelReferences.s.buildingDestroyedParticles, transform);
                hpState = HPLowStates.destroyed;
            }
        } else {
            if (hpPercent < 0.25f) {
                if (hpState != HPLowStates.critical) {
                    if(activeHPCriticalIndicatorEffects != null)
                        activeHPCriticalIndicatorEffects.GetComponent<SmartDestroy>().Engage();
                    activeHPCriticalIndicatorEffects = Instantiate(LevelReferences.s.buildingHPCriticalParticles, transform);
                    hpState = HPLowStates.critical;
                }
            }else if (hpPercent < 0.5f) {
                if (hpState != HPLowStates.low) {
                    if(activeHPCriticalIndicatorEffects != null)
                        activeHPCriticalIndicatorEffects.GetComponent<SmartDestroy>().Engage();
                    activeHPCriticalIndicatorEffects = Instantiate(LevelReferences.s.buildingHPLowParticles, transform);
                    hpState = HPLowStates.low;
                }
            } else {
                if (hpState != HPLowStates.full) {
                    if(activeHPCriticalIndicatorEffects != null)
                        activeHPCriticalIndicatorEffects.GetComponent<SmartDestroy>().Engage();
                    activeHPCriticalIndicatorEffects = null;
                    hpState = HPLowStates.full;
                }
            }
        }
    }

    public void Repair(float heal) {
        Assert.IsTrue(heal > 0);

        if (currentHealth < maxHealth) {
            currentHealth += heal;

            Instantiate(LevelReferences.s.repairEffectPrefab, GetUITransform());

            if (myBuilding.isDestroyed && currentHealth > maxHealth / 2) {
                GetUnDestroyed();
            }

            if (currentHealth > maxHealth) {
                currentHealth = maxHealth;
            }


            UpdateHPCriticalIndicators();

            SetBuildingShaderHealth(currentHealth / maxHealth);
        }
    }

    public void SetHealth(float health) {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        SetBuildingShaderHealth(currentHealth / maxHealth);
        
        if(currentHealth <= 0) {
            var repairable = GetComponent<RepairableIfDestroyed>();
            if (repairable != null) {
                GetDestroyed();
            } else {
                Die();
            }
        }
        
        myBuilding = GetComponent<Cart>();
        if (myBuilding.isDestroyed && currentHealth > maxHealth / 2) {
            GetUnDestroyed();
        }
        
        
        UpdateHPCriticalIndicators();
    }

    [ShowIf("damageNearCartsOnDeath")]
    public int[] explosionDamages = new[] { 100, 50, 25 };
    void DamageNearCartsOnDeath() {
        var myModule = GetComponent<Cart>();

        var forwardWave = myModule;
        for (int i = 0; i < 3; i++) {
            forwardWave = Train.s.GetNextBuilding(true, forwardWave);
            if(forwardWave == null)
                break;
            
            GameObject prefab;
            switch (i) {
                case 0:
                    prefab = LevelReferences.s.bigDamagePrefab;
                    break;
                case 1:
                    prefab = LevelReferences.s.mediumDamagePrefab;
                    break;
                case 2:
                    prefab = LevelReferences.s.smallDamagePrefab;
                    break;
                default:
                    prefab = LevelReferences.s.smallDamagePrefab;
                    break;
            }
            DealDamageToBuilding(forwardWave, prefab, explosionDamages[i]);
        }
        
        var backwardsWave = myModule;
        for (int i = 0; i < 3; i++) {
            backwardsWave = Train.s.GetNextBuilding(false, myModule);
            if(backwardsWave == null)
                break;
            
            GameObject prefab;
            switch (i) {
                case 0:
                    prefab = LevelReferences.s.bigDamagePrefab;
                    break;
                case 1:
                    prefab = LevelReferences.s.mediumDamagePrefab;
                    break;
                case 2:
                    prefab = LevelReferences.s.smallDamagePrefab;
                    break;
                default:
                    prefab = LevelReferences.s.smallDamagePrefab;
                    break;
            }
            DealDamageToBuilding(backwardsWave, prefab, explosionDamages[i]);
        }


        var range = 1.8f;
        var allEnemies = EnemyWavesController.s.GetComponentsInChildren<EnemyHealth>();

        for (int i = 0; i < allEnemies.Length; i++) {
            var enemy = allEnemies[i];
            var distance = Vector3.Distance(enemy.transform.position, transform.position);
            if (distance < range) {
                distance = Mathf.Clamp(distance, range/3, range);
                var damage = distance.Remap(range/3, range, 100, 25);

                GameObject prefab = LevelReferences.s.smallDamagePrefab;
                if (damage > 80) {
                    prefab = LevelReferences.s.bigDamagePrefab;
                }else if (damage > 50) {
                    prefab = LevelReferences.s.mediumDamagePrefab;
                }

                var point = enemy.GetMainCollider().ClosestPoint(transform.position);

                Instantiate(prefab, point, Quaternion.identity);
                enemy.DealDamage(damage);

            }
        }
    }


    private void DealDamageToBuilding(Cart building, GameObject prefab, float damage) {
        if (building == null)
            return;

        var hp = building.GetComponent<ModuleHealth>();
        if (hp != null) {
            hp.DealDamage(damage);
            Instantiate(prefab, hp.transform.position, Quaternion.identity);
        }
    }

    public bool burnResistant = false;
    float burnReduction = 0.5f;
    public float currentBurn = 0;
    public float burnSpeed = 0;
    private float lastBurn;
    public void BurnDamage(float damage) {
        if (burnResistant)
            damage /= 2;
        
        burnSpeed += damage;
    }
    private void Update() {
        var burnDistance = Mathf.Max(burnSpeed / 2f, 1f);
        if (currentBurn >= burnDistance) {
            Instantiate(LevelReferences.s.damageNumbersPrefab, LevelReferences.s.uiDisplayParent)
                .GetComponent<MiniGUI_DamageNumber>()
                .SetUp(GetGameObject().transform, burnDistance, true, false, true);
            DealDamage(burnDistance);

            currentBurn -= burnDistance;
        }

        if (burnSpeed > 0.05f) {
            currentBurn += burnSpeed * Time.deltaTime;
        }
        
        burnSpeed = Mathf.Lerp(burnSpeed,0,burnReduction*Time.deltaTime);


        if (PlayStateMaster.s.isCombatInProgress()) {
            if (selfDamage) {
                selfDamageTimer -= Time.deltaTime;
                if (selfDamageTimer <= 0) {
                    selfDamageTimer = 10;

                    SelfDamage();
                }
            }
        }
        
        if (Mathf.Abs(lastBurn - burnSpeed) > 1 || (lastBurn > 0 && burnSpeed <= 0)) {
            SetBuildingShaderBurn(burnSpeed);
            lastBurn = burnSpeed;
        }
    }

    void SelfDamage() {
        var myModule = GetComponent<Cart>();

        var multiplier = TweakablesMaster.s.myTweakables.engineOverloadDamageMultiplier;
        DealDamage(selfDamageAmounts[0] * multiplier);
        var prefab = LevelReferences.s.smallDamagePrefab;
        Instantiate(prefab, transform.position, Quaternion.identity);
        
        DealDamageToBuilding(Train.s.GetNextBuilding(true, myModule), prefab, selfDamageAmounts[1] * multiplier);
        DealDamageToBuilding(Train.s.GetNextBuilding(false, myModule), prefab, selfDamageAmounts[1] * multiplier);
    }
    
    private void Start() {
        if (GetComponentInParent<Train>() != null) {
            //healthBar = Instantiate(LevelReferences.s.partHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
            //healthBar.SetUp(this, GetComponent<ModuleAmmo>());
            buildingsBuild += 1;
        }

        myBuilding = GetComponent<Cart>();
        
        myBar.UpdateHealth(currentHealth/maxHealth);
    }

    [NonSerialized]
    public UnityEvent dieEvent = new UnityEvent();
    
    [Button]
    public void Die() {
        /*if (damageNearCartsOnDeath) {
            DamageNearCartsOnDeath();
        }*/

        if (isCartHp) {
            var buildings = GetComponentsInChildren<Cart>();
            for (int i = 0; i < buildings.Length; i++) {
                buildings[i].GetComponent<ModuleHealth>().Die();
            }
        }
        
        isDead = true;
        Instantiate(explodePrefab, transform.position, transform.rotation);
        SoundscapeController.s.PlayModuleExplode();

        if (GetComponentInParent<Train>() != null) {
            buildingsDestroyed += 1;
        }

        // in case of death give some of the cost back
        var trainBuilding = GetComponent<Cart>();
        if(trainBuilding)
            LevelReferences.s.SpawnResourceAtLocation(ResourceTypes.scraps, trainBuilding.cost * 0.25f, transform.position);
        
        
        dieEvent?.Invoke();
        
        Destroy(gameObject);
    }

    [Button]
    public void GetDestroyed() {
        myBuilding.isDestroyed = true;
        GetComponent<PossibleTarget>().enabled = false;

        var engineModule = GetComponent<EngineModule>();
        if (engineModule) {
            engineModule.enabled = false;
            GetComponentInChildren<EngineFireController>().StopEngineFire();
        }

        var gunModule = GetComponent<GunModule>();
        if (gunModule) {
            if (gunModule.beingDirectControlled) {
                DirectControlMaster.s.DisableDirectControl();
            }

            gunModule.DeactivateGun();

            GetComponent<TargetPicker>().enabled = false;
        }


        var colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = false;
        }

        SetBuildingShaderAlive(false);

        Instantiate(explodePrefab, transform.position, transform.rotation);
        SoundscapeController.s.PlayModuleExplode();
        
        if (damageNearCartsOnDeath) {
            DamageNearCartsOnDeath();
        }
    }

    [Button]
    public void GetUnDestroyed() {
        myBuilding.isDestroyed = false;
        GetComponent<PossibleTarget>().enabled = true;
        
        var engineModule = GetComponent<EngineModule>();
        if (engineModule) {
            engineModule.enabled = true;
            GetComponentInChildren<EngineFireController>().ActivateEngineFire();
        }

        var gunModule = GetComponent<GunModule>();
        if (gunModule) {
            gunModule.ActivateGun();
            GetComponent<TargetPicker>().enabled = true;
        }
        

        var colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++) {
            colliders[i].enabled = true;
        }

        SetBuildingShaderAlive(true);
    }
    
    void SetBuildingShaderHealth(float value) {
        var _renderers = GetComponentsInChildren<MeshRenderer>();
        for (int j = 0; j < _renderers.Length; j++) {
            var rend = _renderers[j];
            if (rend != null) {
                rend.material.SetFloat("_Health", value);
            }
        }
    }
    
    void SetBuildingShaderBurn(float value) {
        var _renderers = GetComponentsInChildren<MeshRenderer>();
        value = value.Remap(0, 10, 0, 0.5f);
        value = Mathf.Clamp(value, 0, 2f);
        for (int j = 0; j < _renderers.Length; j++) {
            var rend = _renderers[j];
            if (rend != null) {
                rend.material.SetFloat("_Burn", value);
            }
        }
    }

    void SetBuildingShaderAlive(bool isAlive) {
        var _renderers = GetComponentsInChildren<MeshRenderer>();
        var value = isAlive ? 1f : 0.172f;
        for (int j = 0; j < _renderers.Length; j++) {
            var rend = _renderers[j];
            if (rend != null) {
                rend.material.SetFloat("_Alive", value);
            }
        }
    }

    private void OnDestroy() {
        if(healthBar != null)
            if(healthBar.gameObject != null)
                Destroy(healthBar.gameObject);
    }

    public bool IsPlayer() {
        return true;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }
    
    public Collider GetMainCollider() {
        return GetComponentInChildren<BoxCollider>();
    }

    public bool HasArmor() {
        return false;
    }

    public float GetHealthPercent() {
        return currentHealth / maxHealth;
    }
    public string GetHealthRatioString() {
        return $"{currentHealth}/{maxHealth}";
    }

    public Transform GetUITransform() {
        myBuilding = GetComponent<Cart>();
        if (myBuilding != null) {
            return myBuilding.GetUITargetTransform();
        } else {
            return transform;
        }
    }

    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void ActivateForShopping() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
