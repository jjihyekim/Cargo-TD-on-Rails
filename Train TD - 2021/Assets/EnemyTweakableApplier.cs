using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyTweakableApplier : MonoBehaviour {
    
    [ValueDropdown("GetAllEnemyNames")]
    public string uniqueName = "unset";

    private void OnEnable() {
        //ApplyStats(DataHolder.s.GetTweaks());
        //DataHolder.s.tweakableChanged.AddListener(ApplyStats);
    }

    private void OnDisable() {
        //DataHolder.s.tweakableChanged.RemoveListener(ApplyStats);
    }
    
    private void ApplyStats(TweakablesParent tweakablesParent) {
        TweakableEnemy myTweaks = null;
        for (int i = 0; i < tweakablesParent.enemies.Length; i++) {
            if (tweakablesParent.enemies[i].uniqueName == uniqueName) {
                myTweaks = tweakablesParent.enemies[i];
            }
        }

        if (myTweaks == null) {
            Debug.LogError($"Tweaks for enemy:{uniqueName} not found!");
            return;
        }

        var swarmMaker = GetComponent<EnemySwarmMaker>();
        swarmMaker.speed = myTweaks.enemySpeed;

        var enemy = swarmMaker.enemyPrefab;
        var hp = enemy.GetComponent<EnemyHealth>();
        
        var hpPercent = hp.currentHealth / hp.maxHealth;
        hp.maxHealth = myTweaks.enemyHealth;
        hp.currentHealth = myTweaks.enemyHealth*hpPercent;

        hp.ammoReward = myTweaks.enemyAmmoReward;
        hp.fuelReward = myTweaks.enemyFuelReward;
        hp.scrapReward = myTweaks.enemyScrapReward;
        
        myTweaks.enemyGun.ApplyTweaks(enemy.GetComponent<GunModule>());
    }

    
    private static IEnumerable GetAllEnemyNames() {
        var enemies = GameObject.FindObjectOfType<DataHolder>().enemies;
        var enemyNames = new List<string>();
        for (int i = 0; i < enemies.Length; i++) {
            enemyNames.Add(enemies[i].uniqueName);
        }
        return enemyNames;
    }
}
