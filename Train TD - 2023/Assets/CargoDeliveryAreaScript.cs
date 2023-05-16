using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CargoDeliveryAreaScript : MonoBehaviour {

    public SnapCartLocation location1;
    public SnapCartLocation location2;

    public Transform rotatingPlatform;

    
    void Update()
    {
        if (!isEngaged && !PlayerWorldInteractionController.s.isDragStarted) {
            if (location1.snapTransform.childCount > 0) {
                StartCoroutine(EngagePlatform(location1, location2));
            } else if (location2.snapTransform.childCount > 0) {
                StartCoroutine(EngagePlatform(location2, location1));
            }
        }
    }

    public float rotateSpeed = 20;
    public float rotateAcceleration = 20;

    private bool isEngaged = false;
    IEnumerator EngagePlatform(SnapCartLocation fullPlatform, SnapCartLocation emptyPlatform) {
        isEngaged = true;
        SetColliderStatus(fullPlatform.gameObject, false);

        //yield return null; //wait a frame for good luck

        var cargoModule = fullPlatform.GetComponentInChildren<CargoModule>();
        var reward = DataHolder.s.GetCart(cargoModule.GetReward());
        var rewardCart = Instantiate(reward.gameObject, emptyPlatform.snapTransform);
        
        SetColliderStatus(fullPlatform.gameObject, false);

        var rotateTarget = (rotatingPlatform.rotation.eulerAngles.y > 25) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
        var totalDelta = Quaternion.Angle(rotatingPlatform.rotation, rotateTarget);
        var currentDelta = Quaternion.Angle(rotatingPlatform.rotation, rotateTarget);
        var curRotateSpeed = 0f;

        while (currentDelta > 0.1f) {
            rotatingPlatform.rotation = Quaternion.RotateTowards(rotatingPlatform.rotation, rotateTarget, curRotateSpeed * Time.deltaTime);
            curRotateSpeed += rotateAcceleration * Time.deltaTime;
            curRotateSpeed = Mathf.Clamp(curRotateSpeed, 0, rotateSpeed);

            currentDelta = Quaternion.Angle(rotatingPlatform.rotation, rotateTarget); 
            yield return null;
        }

        rotatingPlatform.rotation = rotateTarget;
        
        SetColliderStatus(fullPlatform.gameObject, true);
        Destroy(fullPlatform.snapTransform.GetChild(0).gameObject);
        
        UpgradesController.s.AddCartToShop(rewardCart.GetComponent<Cart>(), UpgradesController.CartLocation.world);
        UpgradesController.s.RemoveCartFromShop(fullPlatform.snapTransform.GetChild(0).GetComponent<Cart>());
        
        rewardCart.transform.SetParent(null);
        rewardCart.GetComponent<Rigidbody>().isKinematic = false;
        rewardCart.GetComponent<Rigidbody>().useGravity = true;

        isEngaged = false;
    }

    void SetColliderStatus(GameObject target, bool status) {
        var allColliders = target.GetComponentsInChildren<Collider>();
        for (int i = 0; i < allColliders.Length; i++) {
            allColliders[i].enabled = status;
        }
    }
}
