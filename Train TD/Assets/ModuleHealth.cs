using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
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

    private void Start() {
        healthBar = Instantiate(LevelReferences.s.partHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
        healthBar.SetUp(this, GetComponent<ModuleAmmo>());
        buildingsBuild += 1;
    }

    public void Die() {
        isDead = true;
        Instantiate(explodePrefab, transform.position, transform.rotation);
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
        
        buildingsDestroyed += 1;
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
