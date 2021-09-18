using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModuleOptionSelector : MonoBehaviour
{
    
    
/*void CastRayToActivateBuildingOptions() {
    RaycastHit hit;
    Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

    if (Physics.Raycast(ray, out hit, 100f, buildingLayerMask)) {
        var slot = hit.collider.gameObject.GetComponentInParent<Slot>();

        if (slot != activeSlot) {
            var index = NormalToIndex(hit.normal);
            
            activeSlot = slot;
            if (index != -1 && index != lastRaycastIndex) {
                activeIndex = index;
                lastRaycastIndex = index;
            }

            activeIndex = tempBuilding.SetRotationBasedOnIndex(activeIndex);
            UpdateCanBuildable();
            
        } else { //if it is still the same slot but a different side, then rotate our building
            var index = NormalToIndex(hit.normal);

            if (index != -1 && index != lastRaycastIndex) {
                lastRaycastIndex = index;
                activeIndex = index;
                activeIndex = tempBuilding.SetRotationBasedOnIndex(index);
                UpdateCanBuildable();
            }
        }
    } else {
        activeSlot = null;
    }
}*/
}
