using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunUpgradeModule : MonoBehaviour, IActiveDuringCombat {
    private float curResupplyDelay = 0.5f;
    public float resupplyDelay = 2;
    public int resupplyAmount = 25;
    
    private Cart myCart;
    private Train myTrain;

    public GameObject resupplyPrefab;
    private void Start() {
        myCart = GetComponentInParent<Cart>();
        myTrain = GetComponentInParent<Train>();

        if (myCart == null || myTrain == null)
            this.enabled = false;
    }

    void Update() {
        if (curResupplyDelay <= 0) {
            BreadthFirstResupplySearch();
            curResupplyDelay = resupplyDelay;
        } else {
            curResupplyDelay -= Time.deltaTime;
        }
    }


    void BreadthFirstResupplySearch() {
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
            if (ResupplyInCart(carts[i], false)) {
                return;
            }
        }
        
        for (int i = 0; i < carts.Count; i++) {
            if (ResupplyInCart(carts[i], true)) {
                return;
            }
        }
    }
    
    private bool inBounds <T>(int index, List<T> array) 
    {
        return (index >= 0) && (index < array.Count);
    }

    bool ResupplyInCart(Cart target, bool resupplyImperfect) {
        var resupplyAbles = target.GetComponentsInChildren<IResupplyAble>();

        if (resupplyAbles.Length > 0) {
            for (int i = 0; i < resupplyAbles.Length; i++) {
                var canRepair = (resupplyAbles[i].MissingSupplies() > 0 && resupplyImperfect) ||
                                resupplyAbles[i].MissingSupplies() >= resupplyAmount;

                if (canRepair) {
                    resupplyAbles[i].Resupply(resupplyAmount);
                    Instantiate(resupplyPrefab, resupplyAbles[i].GetResupplyEffectSpawnTransform().position, Quaternion.identity);
                    return true;
                }
            }
        }

        return false;
    }
    
    public void ActivateForCombat() {
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
}


interface IResupplyAble {
    public void Resupply(int amount);
    public int MissingSupplies();

    public Transform GetResupplyEffectSpawnTransform();
}