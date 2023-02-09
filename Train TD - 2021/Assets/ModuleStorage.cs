using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModuleStorage : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {
    public ResourceTypes myType;
    public int amount = 100;
    public float generationPerSecond = 0.1f;

    private void OnEnable() {
        var train = GetComponentInParent<Train>();
        
        if(train != null)
            train.ReCalculateStorageAmounts();
    }


    private int chunkAmount = 5;
    private float curAmount = 0;

    private void Update() {
        if (SceneLoader.s.isLevelInProgress) {
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
