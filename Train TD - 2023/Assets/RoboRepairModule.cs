using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoboRepairModule : MonoBehaviour, IActiveDuringCombat, IActivateWhenAttachedToTrain {
    private float curRepairDelay = 0.5f;
    public float repairDelay = 2;
    public float repairAmount = 25;
    public float steamUsePerRepair = 0.5f;

    public int repairRange = 1;
    public int repairPerCycle = 2;
    
    private Train myTrain;
    private Cart myCart;
    private void Start() {
        myTrain = GetComponentInParent<Train>();
        myCart = GetComponentInParent<Cart>();
        
        if (myTrain == null)
            this.enabled = false;
    }

    void Update() {
        myTrain = GetComponentInParent<Train>();
        
        if (myTrain == null)
            this.enabled = false;
        if (PlayStateMaster.s.isCombatInProgress()) {
            if (curRepairDelay <= 0 && !myCart.isDestroyed) {
                if(BreadthFirstRepairSearch())
                    SpeedController.s.UseSteam(steamUsePerRepair);
                curRepairDelay = repairDelay;
            } else {
                curRepairDelay -= Time.deltaTime;
            }
        }
    }


    bool BreadthFirstRepairSearch() {
        if(myTrain == null)
            myTrain = GetComponentInParent<Train>();
        var carts = new List<Cart>();

        var range = Mathf.Min(myTrain.carts.Count, repairRange);
        
        for (int i = 0; i < range; i++) {

            if (inBounds(myCart.trainIndex - i, myTrain.carts)) {
                var cart = myTrain.carts[myCart.trainIndex - i].GetComponent<Cart>();
                if(!carts.Contains(cart))
                    carts.Add(cart);
            }
            if (inBounds(myCart.trainIndex + i, myTrain.carts)) {
                var cart = myTrain.carts[myCart.trainIndex + i].GetComponent<Cart>();
                if(!carts.Contains(cart))
                    carts.Add(cart);
            }
        }

        var repairCount = 0;
        for (int i = 0; i < carts.Count; i++) {
            if (RepairDamageInCart(carts[i], false)) {
                repairCount += 1;
                if (repairCount >= repairPerCycle) {
                    return true;
                }
            }
        }
        
        for (int i = 0; i < carts.Count; i++) {
            if (RepairDamageInCart(carts[i], true)) {
                if (repairCount >= repairPerCycle) {
                    return true;
                }
            }
        }

        if (repairCount > 0) {
            return true;
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
                    healths[i].Repair(repairAmount);
                    return true;
                }
            }
        }

        return false;
    }

    public void ActivateForCombat() {
        myTrain = GetComponentInParent<Train>();
        myCart = GetComponent<Cart>();
        this.enabled = true;
    }

    public void Disable() {
        this.enabled = false;
    }
    
    public GameObject attachmentThing;
    public List<GameObject> attachmentThings = new List<GameObject>();

    public bool isAttached = false;

    public void AttachedToTrain() {
        if (isAttached == false) {
            isAttached = true;

            ApplyAmmoBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), true);
            ApplyAmmoBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), true);
        }
    }


    void ApplyAmmoBoost(Cart target, bool doApply) {
        if(target == null)
            return;
        
        var healModule = target.GetComponentInChildren<ModuleHealth>();

        if (healModule != null) {
            if (doApply) {
                attachmentThings.Add(
                    Instantiate(attachmentThing).GetComponent<AttachmentThingScript>().SetUp(GetComponentInParent<Cart>(), target)
                );
            } else {
            }
        }
    }

    public void DetachedFromTrain() {
        if (isAttached == true) {
            isAttached = false;
			
            DeleteAllAttachments();

            ApplyAmmoBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), false);
            ApplyAmmoBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), false);
        }
    }
    
    void DeleteAllAttachments() {
        for (int i = 0; i < attachmentThings.Count; i++) {
            if (attachmentThings[i] != null) {
                Destroy(attachmentThings[i]);
            }
        }
        
        attachmentThings.Clear();
    }

    private void OnDestroy() {
        DeleteAllAttachments();
    }
}
