using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCartReward : MonoBehaviour
{
    private Rigidbody rg;
    
    public Vector3 target = Vector3.zero;
    
    public float lerpSpeed = 3f;

    float lookRotationDelta = 20f;
    
    private void Start() {
        rg = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        UpdatePosition();
    }
    
    private void UpdatePosition() {
        target.y = transform.localPosition.y;

        transform.localPosition = Vector3.Lerp(transform.localPosition, target, lerpSpeed * Time.deltaTime);

        if (transform.position.y < -1) {
            transform.position += Vector3.up*20;
            rg.velocity = Vector3.zero;
        }

        var lookRotation = rg.velocity;
        lookRotation.z = 0.5f;
        lookRotation.y = 0;
        lookRotation.x = Mathf.Clamp(lookRotation.x, -0.2f, 0.2f);
        var realLookRotation = Quaternion.LookRotation(lookRotation, Vector3.up);
        var eulerAngles = transform.rotation.eulerAngles;
        /*eulerAngles.x = Mathf.Clamp(eulerAngles.x, -45, 45);
        eulerAngles.z = Mathf.Clamp(eulerAngles.x, -45, 45);*/
        var adjustedLookRotation = Quaternion.Euler(eulerAngles.x, realLookRotation.y, eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, adjustedLookRotation, lookRotationDelta * Time.deltaTime);
    }

    float GetRealMoveSpeed() {
        return Mathf.Clamp(LevelReferences.s.speed/3f, -2, 2);
    }

    public void RewardPlayerCart() {
        var rewardedCart = Instantiate(LevelReferences.s.emptyCart);
        Train.s.AddCartAtIndex(Train.s.carts.Count, rewardedCart.GetComponent<Cart>());
    }
}
