using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModuleStorage : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {
    public ResourceTypes myType;
    public int amount = 100;
    public float generationPerSecond = 0.1f;

    private Cart myBuilding;
    private void OnEnable() {
        var train = GetComponentInParent<Train>();

        myBuilding = GetComponent<Cart>();
    }


    private int chunkAmount = 1;
    private float curAmount = 0;

    private void Update() {
        if (PlayStateMaster.s.isCombatInProgress() && !myBuilding.isDestroyed) {
            curAmount += generationPerSecond * Time.deltaTime;
            if (curAmount > chunkAmount) {
                curAmount -= chunkAmount;
                
                LevelReferences.s.SpawnResourceAtLocation(myType, chunkAmount, transform.position);
            }
        }
    }

    private void OnDestroy() {
        var train = GetComponentInParent<Train>();
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
