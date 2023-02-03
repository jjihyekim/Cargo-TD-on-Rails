using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModuleHealth : MonoBehaviour, IHealth, IActiveDuringCombat, IActiveDuringShopping {

    public float maxHealth = 50;
    public float currentHealth = 50;


    [ReadOnly]
    public MiniGUI_HealthBar healthBar;
    
    public GameObject explodePrefab;
    public bool isDead = false;
    
    
    public static int buildingsBuild;
    public static int buildingsDestroyed;

    public bool damageNearCartsOnDeath = false;
    public float selfDamageMultiplier = 1;
    public bool selfDamage = false;
    [ShowIf("selfDamage")] 
    private float selfDamageTimer;
    public int[] selfDamageAmounts = new[] { 20, 10 };

    private TrainBuilding myBuilding;

    private GameObject activeHPCriticalIndicatorEffects;
    private enum HPLowStates {
        full, low, critical, destroyed
    }

    private HPLowStates hpState;
    [Button]
    public void DealDamage(float damage) {
        Assert.IsTrue(damage > 0);
        if (!isDead) {
            currentHealth -= damage;

            var hpPercent = currentHealth / maxHealth;
            SetBuildingShaderHealth(hpPercent);
            
            if(currentHealth <= 0) {
                var repairable = GetComponent<RepairableIfDestroyed>();
                if (repairable != null) {
                    GetDestroyed();
                } else {
                    Die();
                }
            }

            UpdateHPCriticalIndicators();

            if (currentHealth < 0) {
                currentHealth = 0;
            }
        }
    }

    void UpdateHPCriticalIndicators() {
        var hpPercent = currentHealth / maxHealth;
        if(isDead)
            return;
        myBuilding = GetComponent<TrainBuilding>();
        if (myBuilding == null) {
            Debug.LogError("Building is missing!");
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

    public void Heal(float heal) {
        Assert.IsTrue(heal > 0);
        currentHealth += heal;

        if (myBuilding.isDestroyed && currentHealth > maxHealth / 2) {
            GetUnDestroyed();
        }
        
        if (currentHealth > maxHealth) {
            currentHealth = maxHealth;
        }
        
        
        UpdateHPCriticalIndicators();

        SetBuildingShaderHealth(currentHealth / maxHealth);
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
        
        myBuilding = GetComponent<TrainBuilding>();
        if (myBuilding.isDestroyed && currentHealth > maxHealth / 2) {
            GetUnDestroyed();
        }
        
        
        UpdateHPCriticalIndicators();
    }

    [ShowIf("damageNearCartsOnDeath")]
    public int[] explosionDamages = new[] { 100, 50, 25 };
    void DamageNearCartsOnDeath() {
        var myModule = GetComponent<TrainBuilding>();

        var forwardWave = myModule.mySlot;
        for (int i = 0; i < 3; i++) {
            forwardWave = GetNextSlot(true, forwardWave);
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
            DealDamageToSlot(forwardWave, prefab, explosionDamages[i]);
        }
        
        var backwardsWave = myModule.mySlot;
        for (int i = 0; i < 3; i++) {
            backwardsWave = GetNextSlot(false, backwardsWave);
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
            DealDamageToSlot(backwardsWave, prefab, explosionDamages[i]);
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

    Slot GetNextSlot(bool isForward, Slot slot) {
        if (isForward) {
            if (slot.isFrontSlot) {
                var nextCart = slot.GetCart().index - 1;
                if (nextCart > 0) {
                    return Train.s.carts[nextCart].GetComponent<Cart>().backSlot;
                } else {
                    return null;
                }
            } else {
                return slot.GetCart().frontSlot;
            }
        } else {
            if (!slot.isFrontSlot) {
                var nextCart = slot.GetCart().index + 1;
                if (nextCart < Train.s.carts.Count) {
                    return Train.s.carts[nextCart].GetComponent<Cart>().frontSlot;
                } else {
                    return null;
                }
            } else {
                return slot.GetCart().backSlot;
            }
        }
    }

    void DealDamageToSlot(Slot slot, GameObject prefab, int damage) {
        if(slot == null)
            return;
        
        var buildings = slot.myBuildings;
        for (int i = 0; i < buildings.Length; i++) {
            if (buildings[i] != null) {
                var hp = buildings[i].GetComponent<ModuleHealth>();
                if (hp != null) {
                    hp.DealDamage(damage);
                    Instantiate(prefab, hp.transform.position, Quaternion.identity);
                }
                
                if(buildings[i].occupiesEntireSlot) // skip the entire rest of the slot.
                    return;
                if (i == 0) {//skip the other top slot
                    i++;
                }
            }
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
                .SetUp(GetGameObject().transform, (int)1, true, false, true);
            DealDamage(1);

            currentBurn = 0;
        }

        if (burnSpeed > 0.05f) {
            currentBurn += burnSpeed * Time.deltaTime;
        }
        
        burnSpeed = Mathf.Lerp(burnSpeed,0,burnReduction*Time.deltaTime);


        if (SceneLoader.s.isLevelInProgress) {
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
        var myModule = GetComponent<TrainBuilding>();
        
        DealDamage(selfDamageAmounts[0]);
        var prefab = LevelReferences.s.smallDamagePrefab;
        Instantiate(prefab, transform.position, Quaternion.identity);
        
        DealDamageToSlot(GetNextSlot(true, myModule.mySlot), prefab, selfDamageAmounts[1]);
        DealDamageToSlot(GetNextSlot(false, myModule.mySlot), prefab, selfDamageAmounts[1]);
    }
    
    private void Start() {
        if (GetComponentInParent<Train>() != null) {
            healthBar = Instantiate(LevelReferences.s.partHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
            healthBar.SetUp(this, GetComponent<ModuleAmmo>());
            buildingsBuild += 1;
        }

        myBuilding = GetComponent<TrainBuilding>();
    }

    [NonSerialized]
    public UnityEvent dieEvent = new UnityEvent();
    
    [Button]
    public void Die() {
        if (damageNearCartsOnDeath) {
            DamageNearCartsOnDeath();
        }
        
        isDead = true;
        Instantiate(explodePrefab, transform.position, transform.rotation);
        SoundscapeController.s.PlayModuleExplode();
        Destroy(healthBar.gameObject);

        if (GetComponentInParent<Train>() != null) {
            buildingsDestroyed += 1;
        }

        // in case of death give some of the cost back
        var trainBuilding = GetComponent<TrainBuilding>();
        if(trainBuilding)
            LevelReferences.s.SpawnResourceAtLocation(ResourceTypes.scraps, trainBuilding.cost * 0.25f, transform.position);

        var cart = GetComponent<Cart>();
        if(cart)
            LevelReferences.s.SpawnResourceAtLocation(ResourceTypes.scraps,  25f, transform.position);
        
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
        
        GetComponentInParent<Cart>().SlotsAreUpdated();
        
        Instantiate(explodePrefab, transform.position, transform.rotation);
        SoundscapeController.s.PlayModuleExplode();
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
        
        GetComponentInParent<Cart>().SlotsAreUpdated();
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
        value = value.Remap(0, 10, 0, 1);
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
        return GetComponent<TrainBuilding>().GetUITargetTransform(false);
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
