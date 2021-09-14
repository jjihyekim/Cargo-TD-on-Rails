using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModuleHealth : MonoBehaviour, IHealth {

    public float maxHealth = 50;
    public float currentHealth = 50;


    public MiniGUI_HealthBar healthBar;
    
    public GameObject explodePrefab;
    public bool isDead = false;
    
    

    public static int buildingsBuild;
    public static int buildingsDestroyed;
    
    public void DealDamage(float damage) {
        currentHealth -= damage;
        if (!isDead && currentHealth <= 0) {
            Die();
        }
    }

    private void Start() {
        healthBar = Instantiate(LevelReferences.s.partHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
        healthBar.SetUp(this);
        buildingsBuild += 1;
    }

    public void Die() {
        isDead = true;
        Instantiate(explodePrefab, transform.position, transform.rotation);
        Destroy(healthBar.gameObject);
        Destroy(gameObject);
        
        buildingsDestroyed += 1;
    }

    public bool IsPlayer() {
        return true;
    }
}
