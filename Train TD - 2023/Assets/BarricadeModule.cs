using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeModule : MonoBehaviour, IActivateWhenAttachedToTrain
{
    public GameObject attachmentThing;
    public List<GameObject> attachmentThings = new List<GameObject>();

    public bool isAttached = false;
    
    public void AttachedToTrain() {
        if (isAttached == false) {
            isAttached = true;

            ApplyBarricade(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), true);
            ApplyBarricade(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), true);
        }
    }


    void ApplyBarricade(Cart target, bool doApply) {
        if(target == null)
            return;
    
        var healthModule = target.GetComponent<ModuleHealth>();

        if (healthModule != null) {
            if (doApply) {
                healthModule.damageDefenders.Add(ProtectFromDamage);
                healthModule.burnDefenders.Add(ProtectFromBurn);

                attachmentThings.Add(
                    Instantiate(attachmentThing).GetComponent<AttachmentThingScript>().SetUp(GetComponentInParent<Cart>(), target)
                );
                
            } else {
                healthModule.damageDefenders.Remove(ProtectFromDamage);
                healthModule.burnDefenders.Remove(ProtectFromBurn);
            }
        }
    }

    public void DetachedFromTrain() {
        if (isAttached == true) {
            isAttached = false;
			
            DeleteAllAttachments();

            ApplyBarricade(Train.s.GetNextBuilding(true, GetComponentInParent<Cart>()), false);
            ApplyBarricade(Train.s.GetNextBuilding(false, GetComponentInParent<Cart>()), false);
        }
    }

    public void ProtectFromDamage(float damage) {
        GetComponentInParent<ModuleHealth>().DealDamage(damage);
    }
    
    public void ProtectFromBurn(float damage) {
        GetComponentInParent<ModuleHealth>().BurnDamage(damage);
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
