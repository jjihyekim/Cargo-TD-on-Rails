using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBoostModule : MonoBehaviour, IActivateWhenAttachedToTrain {

	public int ammoMultiplier = 2;

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
		
		
		var ammo = target.GetComponentInChildren<ModuleAmmo>();

		if (ammo != null) {
			if (doApply) {
				ammo.ChangeMaxAmmo(ammo.maxAmmo * ammoMultiplier);

				attachmentThings.Add(
					Instantiate(attachmentThing).GetComponent<AttachmentThingScript>().SetUp(GetComponentInParent<Cart>(), target)
				);
				
			} else {
				ammo.ChangeMaxAmmo(ammo.maxAmmo / ammoMultiplier);
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
