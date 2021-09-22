using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapPile : MonoBehaviour {

    public float height = 0.5f;

    public int moneyAmount = 0;
    public bool isCollected = false;
    
    
    public void SetUp(int money) {
        moneyAmount = money;
        StickToGround();
        GetComponentInChildren<ScrapPileMaker>().MakePile(moneyAmount/5);
        transform.SetParent(LevelReferences.s.playerTransform);
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

    private void PickTarget() {
        var distance = 500f;
        for (int i = 0; i < LevelReferences.s.carts.Length; i++) {
            var cart = LevelReferences.s.carts[i];
            if (cart != null) {
                var curDist = Vector3.Distance(transform.position, cart.transform.position);
                if (curDist < distance) {
                    distance = curDist;
                    target = cart.center;
                }
            }
        }
    }


    public float speed = 0;
    public float acc = 0.5f;
    public Transform target;
    private void Update() {
        var distance = Vector3.Distance(transform.position, target.position);
        if (isCollected) {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            speed += Time.deltaTime * acc;
            
            if (distance < 0.01f) {
                MoneyController.s.AddMoney(moneyAmount);
                Destroy(gameObject);
            }
        } else {
            // slowly move closer so that we are not outside of the screen ever
            if(distance > 3)
                transform.position = Vector3.MoveTowards(transform.position, target.position, 0.1f * Time.deltaTime);
        }
    }
}
