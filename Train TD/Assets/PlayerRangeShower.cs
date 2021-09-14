using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRangeShower : MonoBehaviour {
    private PlayerBuildingController myCont;
    void Start() {
        myCont = GetComponent<PlayerBuildingController>();
    }


    private void Update() {
        if (LevelLoader.s.isLevelInProgress) {
            CastRayToFindSlot();
        }
    }

    private Slot activeSlot;
    void CastRayToFindSlot() {
        RaycastHit hit;
        Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, 100f, myCont.buildingLayerMask)) {
            var slot = hit.collider.gameObject.GetComponentInParent<Slot>();
            //print(slot.gameObject.name + " - " + slot.GetComponentInParent<Cart>().gameObject.name);

            if (slot != activeSlot) {
                if(activeSlot != null)
                    SetSlotRangesStatus(activeSlot, false);
                SetSlotRangesStatus(slot, true);
                
                activeSlot = slot;
            } 
        } else {
            if (activeSlot != null) 
                SetSlotRangesStatus(activeSlot, false);

            activeSlot = null;
        }
    }

    void SetSlotRangesStatus(Slot slot,bool isActive) {
        var ranges = slot.GetComponentsInChildren<RangeVisualizer>();

        for (int i = 0; i < ranges.Length; i++) {
            ranges[i].ChangeVisualizerStatus(isActive);
        }
    }
}
