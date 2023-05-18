using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBoostModule : MonoBehaviour, IActivateWhenAttachedToTrain
{
    public int damageMultiplier = 2;

    public GameObject attachmentThing;
    public List<GameObject> attachmentThings = new List<GameObject>();

    public bool isAttached = false;
    
    public void AttachedToTrain() {
        if (isAttached == false) {
            isAttached = true;

            ApplyDamageBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), true);
            ApplyDamageBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), true);
        }
    }


    void ApplyDamageBoost(Cart target, bool doApply) {
        if(target == null)
            return;
        
        var gun = target.GetComponentInChildren<GunModule>();

        if (gun != null) {
            if (doApply) {
                gun.projectileDamage *= damageMultiplier;

                attachmentThings.Add(
                    Instantiate(attachmentThing).GetComponent<AttachmentThingScript>().SetUp(GetComponentInParent<Cart>(), target)
                );

            } else {
                gun.projectileDamage /= damageMultiplier;
            }
        }
    }

    public void DetachedFromTrain() {
        if (isAttached == true) {
            isAttached = false;

            DeleteAllAttachments();

            ApplyDamageBoost(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), false);
            ApplyDamageBoost(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), false);
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
