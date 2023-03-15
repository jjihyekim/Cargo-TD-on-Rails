using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModuleStorage : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {
    public ResourceTypes myType;
    public int amount = 100;
    public float generationPerSecond = 0.1f;

    private TrainBuilding myBuilding;
    private void OnEnable() {
        var train = GetComponentInParent<Train>();
        
        if(train != null)
            train.ReCalculateStorageAmounts();

        myBuilding = GetComponent<TrainBuilding>();
    }


    private int chunkAmount = 1;
    private float curAmount = 0;

    private void Update() {
        if (SceneLoader.s.isLevelInProgress && !myBuilding.isDestroyed) {
            curAmount += generationPerSecond * Time.deltaTime;
            if (curAmount > chunkAmount) {
                curAmount -= chunkAmount;
                
                LevelReferences.s.SpawnResourceAtLocation(myType, chunkAmount, transform.position);
            }
        }
    }

    private void OnDestroy() {
        var train = GetComponentInParent<Train>();
        
        if(train != null)
            train.ReCalculateStorageAmounts();
        
        OnModuleDestroyed();
    }

    public void ActivateForCombat() {
        this.enabled = true;

        if (myType == ResourceTypes.ammo) {
            curAmount = amount;
        }
    }

    public void ActivateForShopping() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }

    public void OnModuleDestroyed() {
        MoneyController.s.LoseStorageMaxAmount(myType, amount, transform);
    }

    public void OnModuleUnDestroyed() {
        var train = GetComponentInParent<Train>();
        if(train != null)
            train.ReCalculateStorageAmounts();
    }
}
