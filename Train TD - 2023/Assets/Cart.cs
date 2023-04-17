using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Cart : MonoBehaviour {
    public Transform center;

    public int index;

    public float damageModifier = 1f;
    public float attackSpeedModifier = 1f;

    public int weight = 50;
    
    public GameObject fullCart;
    public GameObject halfCartBehindCut;
    public GameObject halfCartFrontCut;
    public GameObject cutCart;


    public Slot frontSlot;
    public Slot backSlot;

    public Transform uiTargetTransform;

    public BoxCollider myCollider;

    private void Start() {
        SlotsAreUpdated();
    }

    public void SlotsAreUpdated() {
        if (frontSlot.isSlotNeedToBeCut() && backSlot.isSlotNeedToBeCut()) {
            fullCart.SetActive(false);
            halfCartBehindCut.SetActive(false);
            halfCartFrontCut.SetActive(false);
            cutCart.SetActive(true);

        }else if (frontSlot.isSlotNeedToBeCut()) {
            fullCart.SetActive(false);
            halfCartBehindCut.SetActive(false);
            halfCartFrontCut.SetActive(true);
            cutCart.SetActive(false);
            
        }else if (backSlot.isSlotNeedToBeCut()) {
            fullCart.SetActive(false);
            halfCartBehindCut.SetActive(true);
            halfCartFrontCut.SetActive(false);
            cutCart.SetActive(false);
            
        } else {
            fullCart.SetActive(true);
            halfCartBehindCut.SetActive(false);
            halfCartFrontCut.SetActive(false);
            cutCart.SetActive(false);
            
        }

        var isThereAnyBuildingOnCart = false;
        for (int i = 0; i < frontSlot.myBuildings.Length; i++) {
            if (frontSlot.myBuildings[i] != null && !frontSlot.myBuildings[i].isDestroyed)
                isThereAnyBuildingOnCart = true;
        }
        for (int i = 0; i < backSlot.myBuildings.Length; i++) {
            if (backSlot.myBuildings[i] != null&& !backSlot.myBuildings[i].isDestroyed)
                isThereAnyBuildingOnCart = true;
        }

        if (isThereAnyBuildingOnCart) {
            GetComponent<PossibleTarget>().enabled = false;
            myCollider.enabled = false;
        } else {
            GetComponent<PossibleTarget>().enabled = true;
            myCollider.enabled = true;
        }
        
        Train.s.TrainUpdated();
    }

    private void OnDestroy() {
        var train = GetComponentInParent<Train>();

        if (train != null) {
            train.CartDestroyed(this);
        }
    }
}
