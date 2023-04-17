using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboRepairModule : MonoBehaviour, IActiveDuringCombat {
    private float curRepairDelay = 0.5f;
    public float repairDelay = 2;
    public float repairAmount = 25;
    public float steamUsePerRepair = 0.5f;
    
    private Cart myCart;
    private Train myTrain;
    private TrainBuilding myBuilding;
    private void Start() {
        myCart = GetComponentInParent<Cart>();
        myTrain = GetComponentInParent<Train>();
        myBuilding = GetComponent<TrainBuilding>();
        
        if (myCart == null || myTrain == null)
            this.enabled = false;
    }

    void Update() {
        if (SceneLoader.s.isLevelInProgress) {
            if (curRepairDelay <= 0 && !myBuilding.isDestroyed) {
                if(BreadthFirstRepairSearch())
                    SpeedController.s.UseSteam(steamUsePerRepair);
                curRepairDelay = repairDelay;
            } else {
                curRepairDelay -= Time.deltaTime;
            }
        }
    }


    bool BreadthFirstRepairSearch() {
        if(myCart == null)
            myCart = GetComponentInParent<Cart>();
        if(myTrain == null)
            myTrain = GetComponentInParent<Train>();
        var carts = new List<Cart>();
        for (int i = 0; i < myTrain.carts.Count; i++) {

            if (inBounds(myCart.index - i, myTrain.carts)) {
                var cart = myTrain.carts[myCart.index - i].GetComponent<Cart>();
                if(!carts.Contains(cart))
                    carts.Add(cart);
            }
            if (inBounds(myCart.index + i, myTrain.carts)) {
                var cart = myTrain.carts[myCart.index + i].GetComponent<Cart>();
                if(!carts.Contains(cart))
                    carts.Add(cart);
            }
        }

        for (int i = 0; i < carts.Count; i++) {
            if (RepairDamageInCart(carts[i], false)) {
                return true;
            }
        }
        
        for (int i = 0; i < carts.Count; i++) {
            if (RepairDamageInCart(carts[i], true)) {
                return true;
            }
        }

        return false;
    }
    
    private bool inBounds <T>(int index, List<T> array) 
    {
        return (index >= 0) && (index < array.Count);
    }

    bool RepairDamageInCart(Cart target, bool repairImperfect) {
        var healths = target.GetComponentsInChildren<ModuleHealth>();

        if (healths.Length > 0) {
            for (int i = 0; i < healths.Length; i++) {
                var canRepair = (healths[i].currentHealth < healths[i].maxHealth && repairImperfect) ||
                                healths[i].currentHealth <= healths[i].maxHealth - repairAmount;

                if (canRepair) {
                    healths[i].Heal(repairAmount);
                    Instantiate(DataHolder.s.repairPrefab, healths[i].transform.position, Quaternion.identity);
                    return true;
                }
            }
        }

        return false;
    }

    public void ActivateForCombat() {
        myCart = GetComponentInParent<Cart>();
        myTrain = GetComponentInParent<Train>();
        myBuilding = GetComponent<TrainBuilding>();
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}
