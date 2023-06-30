using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CargoDeliveryAreaScript : MonoBehaviour {

    public SnapCartLocation location1;
    public SnapCartLocation location2;

    public Transform rotatingPlatform;

    public Transform regularArtifactArea;
    public Transform bonusArtifactArea;

    public Transform extraArtifactEffectParent;

    void Update()
    {
        if (!isEngaged && !PlayerWorldInteractionController.s.isDragStarted) {
            if (location1.snapTransform.childCount > 0) 
                StartCoroutine(EngagePlatform(location1, location2));
        }
    }

    public float rotateSpeed = 20;
    public float rotateAcceleration = 20;

    private bool isEngaged = false;
    
    IEnumerator EngagePlatform(SnapCartLocation fullPlatform, SnapCartLocation emptyPlatform) {
        isEngaged = true;
        PlayerWorldInteractionController.s.Deselect();
        SetColliderStatus(rotatingPlatform.gameObject, false);

        //yield return null; //wait a frame for good luck

        var cargoModule = fullPlatform.GetComponentInChildren<CargoModule>();
        var rewardCart = Instantiate(DataHolder.s.GetCart(cargoModule.GetRewardCart()).gameObject, emptyPlatform.snapTransform);
        var rewardArtifact = Instantiate(DataHolder.s.GetArtifact(cargoModule.GetRewardArtifact()).gameObject, regularArtifactArea);
        GameObject bonusRewardArtifact = null;
        if (ArtifactsController.s.gotBonusArtifact) {
            bonusRewardArtifact = Instantiate(DataHolder.s.GetArtifact(ArtifactsController.s.bonusArtifactUniqueName).gameObject, bonusArtifactArea);
            ArtifactsController.s.BonusArtifactRewarded(bonusArtifactArea);
        }

        SetColliderStatus(rotatingPlatform.gameObject, false);

        var startRotation = rotatingPlatform.transform.rotation;
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
        
        SetColliderStatus(rotatingPlatform.gameObject, true);
        Destroy(fullPlatform.snapTransform.GetChild(0).gameObject);
        
        UpgradesController.s.AddCartToShop(rewardCart.GetComponent<Cart>(), UpgradesController.CartLocation.world);
        UpgradesController.s.RemoveCartFromShop(fullPlatform.snapTransform.GetChild(0).GetComponent<Cart>());
        
        rewardCart.transform.SetParent(null);
        rewardCart.GetComponent<Rigidbody>().isKinematic = false;
        rewardCart.GetComponent<Rigidbody>().useGravity = true;

        
        FirstTimeTutorialController.s.CargoHintShown();
        
        yield return new WaitForSeconds(0.5f);
        
        rewardArtifact.transform.SetParent(null);
        rewardArtifact.transform.position = regularArtifactArea.position;
        rewardArtifact.GetComponentInChildren<Rigidbody>().isKinematic = false;
        rewardArtifact.GetComponentInChildren<Rigidbody>().useGravity = true;
        rewardArtifact.GetComponentInChildren<Rigidbody>().AddForce(AddNoiseOnDirection(regularArtifactArea.forward, 15) * Random.Range(200,350));

        yield return new WaitForSeconds(0.5f);

        if (bonusRewardArtifact != null) {
            bonusRewardArtifact.transform.SetParent(null);
            bonusRewardArtifact.transform.position = bonusArtifactArea.position;
            bonusRewardArtifact.GetComponentInChildren<Rigidbody>().isKinematic = false;
            bonusRewardArtifact.GetComponentInChildren<Rigidbody>().useGravity = true;
            bonusRewardArtifact.GetComponentInChildren<Rigidbody>().AddForce(AddNoiseOnDirection(bonusArtifactArea.forward, 15) * Random.Range(200,350));

            Instantiate(LevelReferences.s.gotExtraArtifactFromEliteEnemyEffect, extraArtifactEffectParent);

            yield return new WaitForSeconds(0.2f);
        }

        rotatingPlatform.transform.rotation = startRotation;
        
        isEngaged = false;
    }
    
    Vector3 AddNoiseOnDirection (Vector3 direction, float max)  {
        // Generate a random rotation
        Quaternion randomRotation = Quaternion.Euler(Random.Range(-max, max), Random.Range(-max, max), 0);
        
        // Apply the random rotation to the current direction
        return randomRotation * direction;
    }

    void SetColliderStatus(GameObject target, bool status) {
        var allColliders = target.GetComponentsInChildren<Collider>();
        for (int i = 0; i < allColliders.Length; i++) {
            allColliders[i].enabled = status;
        }
    }
}
