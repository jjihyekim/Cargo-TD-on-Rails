using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cart : MonoBehaviour {
    public Transform center;

    public int index;
    
    public float damageModifier = 1f;
    public float attackSpeedModifier = 1f;
    
    public GameObject fullCart;
    public GameObject halfCartBehindCut;
    public GameObject halfCartFrontCut;
    public GameObject cutCart;


    public Slot frontSlot;
    public Slot backSlot;

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
    }
}
