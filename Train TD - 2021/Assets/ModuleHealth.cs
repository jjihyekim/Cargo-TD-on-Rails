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

    public float burnReduction = 0.5f;
    public float currentBurn = 0;
    public float burnSpeed = 0;
    public void BurnDamage(float damage) {
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
    }
    
    private void Start() {
        healthBar = Instantiate(LevelReferences.s.partHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
        healthBar.SetUp(this, GetComponent<ModuleAmmo>());
        buildingsBuild += 1;
    }

    [NonSerialized]
    public UnityEvent dieEvent = new UnityEvent();
    
    public void Die() {
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
