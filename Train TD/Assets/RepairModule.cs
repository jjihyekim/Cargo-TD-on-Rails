using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairModule : MonoBehaviour {
    private float curRepairDelay = 0.5f;
    public float repairDelay = 2;
    public float repairAmount = 25;
    
    private Cart myCart;
    private Train myTrain;

    public GameObject repairPrefab;
    private void Start() {
        myCart = GetComponentInParent<Cart>();
        myTrain = GetComponentInParent<Train>();
        
        
        if (myCart == null || myTrain == null)
            this.enabled = false;
    }

    void Update() {
        if (curRepairDelay <= 0) {
            BreadthFirstRepairSearch();
            curRepairDelay = repairDelay;
        } else {
            curRepairDelay -= Time.deltaTime;
        }
    }


    void BreadthFirstRepairSearch() {
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
                return;
            }
        }
        
        for (int i = 0; i < carts.Count; i++) {
            if (RepairDamageInCart(carts[i], true)) {
                return;
            }
        }
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
                    healths[i].DealDamage(-repairAmount);
                    Instantiate(repairPrefab, healths[i].transform.position, Quaternion.identity);
                    return true;
                }
            }
        }

        return false;
    }
}
