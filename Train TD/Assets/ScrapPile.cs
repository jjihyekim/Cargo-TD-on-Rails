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
        transform.position = new Vector3(transform.position.x, height, transform.position.z);
        GetComponentInChildren<ScrapPileMaker>().MakePile(moneyAmount/5);
        transform.SetParent(LevelReferences.s.playerTransform);
    }


    public void CollectPile() {
        isCollected = true;

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
        if (isCollected) {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            speed += Time.deltaTime * acc;

            var distance = Vector3.Distance(transform.position, target.position);
            if (distance < 0.01f) {
                MoneyController.s.AddMoney(moneyAmount);
                Destroy(gameObject);
            }
        }
    }
}
