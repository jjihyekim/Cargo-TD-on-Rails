using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
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
    public bool selfDamage = false;
    [ShowIf("selfDamage")] 
    private float selfDamageTimer;
    public int[] selfDamageAmounts = new[] { 20, 10 };
    
    [Button]
    public void DealDamage(float damage) {
        if (!isDead) {
            currentHealth -= damage;
            if(currentHealth <= 0) {
                Die();
            }

            if (currentHealth > maxHealth) {
                currentHealth = maxHealth;
            }
        }
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
    public void BurnDamage(float damage) {
        if (burnResistant)
            damage /= 2;
        
        burnSpeed += damage;
    }
    private void Update() {
        if (currentBurn >= 1) {
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
        healthBar = Instantiate(LevelReferences.s.partHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
        healthBar.SetUp(this, GetComponent<ModuleAmmo>());
        buildingsBuild += 1;
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
        
        buildingsDestroyed += 1;
        
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
