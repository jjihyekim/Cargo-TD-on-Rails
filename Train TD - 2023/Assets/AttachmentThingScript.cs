using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is a script to show stuff like ammo boost affecting nearby modules
public class AttachmentThingScript : MonoBehaviour {
    public GameObject SetUp(Cart booster, Cart boosted) {
        gameObject.SetActive(true);
        transform.position = boosted.transform.position;
        transform.SetParent(boosted.transform);

        if (booster.trainIndex > boosted.trainIndex) {
            transform.rotation = Quaternion.Euler(0,180,0);
        }

        return gameObject;
    }
}
