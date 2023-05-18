using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is a script to show stuff like ammo boost affecting nearby modules
public class AttachmentThingScript : MonoBehaviour {
    private bool myRotation;
    public GameObject SetUp(Cart booster, Cart boosted) {
        gameObject.SetActive(true);
        transform.position = boosted.transform.position;
        transform.SetParent(boosted.transform);

        myRotation = booster.trainIndex > boosted.trainIndex;

        if (myRotation) {
            transform.localRotation = Quaternion.Euler(0,180,0);
        } else {
            transform.localRotation = Quaternion.Euler(0,0,0);
        }

        /*if (PlayStateMaster.s.isShop())
            enabled = true;*/

        return gameObject;
    }

    /*private void Update() {
        if (PlayStateMaster.s.isCombatStarted()) {
            enabled = false;
        }
        
        if (booster.trainIndex > boosted.trainIndex) {
            transform.rotation = Quaternion.Euler(0,180,0);
        }
    }*/
}
