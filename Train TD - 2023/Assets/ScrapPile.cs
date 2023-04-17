using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ScrapPile : MonoBehaviour {

    public float height = 0.5f;

    public int scrapAmount = 0;
    public bool isCollected = false;

    public TMP_Text amount;

    public ResourceTypes myType;
    
    public void SetUp(int scrap, ResourceTypes _type) {
        myType = _type;
        scrapAmount = scrap;
        StickToGround();
        GetComponentInChildren<ScrapPileMaker>().MakePile(scrapAmount);
        transform.SetParent(LevelReferences.s.playerTransform);
        amount.text = scrap.ToString();
        PickTarget();
    }

    void StickToGround() {
        var targetHeight = height;
        if (Physics.Raycast(transform.position + Vector3.up * 20, Vector3.down, out RaycastHit hit, 100, LevelReferences.s.groundLayer)) {
            height = hit.point.y + height;
        }

        var curPos = transform.position;
        transform.position = new Vector3(curPos.x, targetHeight, curPos.z);
    }


    public void CollectPile() {
        isCollected = true;
        PickTarget();
    }

    private bool grantAwardToPlayer = true;
    public void CollectPileWithTarget(Transform _target) {
        isCollected = true;
        target = _target;
        grantAwardToPlayer = false;
    }

    private void PickTarget() {
        var distance = float.MaxValue;
        for (int i = 0; i < LevelReferences.s.train.carts.Count; i++) {
            var cart = LevelReferences.s.train.carts[i];
            if (cart != null) {
                var curDist = Vector3.Distance(transform.position, cart.transform.position);
                if (curDist < distance) {
                    distance = curDist;
                    target = cart.GetComponent<Cart>().center;
                }
            }
        }
    }


    public float speed = 0;
    public float acc = 0.5f;
    //public float initialDelay = 0.5f;
    public Transform target;
    private void Update() {
        if (target == null) {
            PickTarget();
            if (target == null) {
                Destroy(gameObject);
                return;
                
            }
        }
        var distance = Vector3.Distance(transform.position, target.position);
        if (isCollected) {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            speed += Time.deltaTime * acc;
            
            if (distance < 0.01f) {
                if(grantAwardToPlayer)
                    MoneyController.s.ModifyResource(myType, scrapAmount);

                Destroy(gameObject);
            }
        } else {
            // slowly move closer so that we are not outside of the screen ever
            if(distance > 3)
                transform.position = Vector3.MoveTowards(transform.position, target.position, 0.1f * Time.deltaTime);
        }
    }
}
