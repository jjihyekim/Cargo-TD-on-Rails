using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Train : MonoBehaviour {
    public static Train s;

    public Transform trainFront;
    public Transform trainBack;
    public Vector3 trainFrontOffset;

    public int cartCount;

    /*public List<Transform> carts = new List<Transform>();
    public List<Vector3> cartDefPositions = new List<Vector3>();*/

    public int GetEmptySlotCount() {
        var slots = GetComponentsInChildren<Slot>();
        int emptyCount = 0;
        for (int i = 0; i < slots.Length; i++) {
            if (slots[i].isCompletelyEmpty())
                emptyCount += 1;
        }

        return emptyCount;
    }

    private void Awake() {
        s = this;
    }


    private void Start() {
        UpdateBasedOnLevelData();
    }

    public void UpdateBasedOnLevelData() {
        var childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }

        LevelReferences.s.carts = null;
        
        /*carts = new List<Transform>();
        cartDefPositions = new List<Vector3>();*/
        
        if(trainFront != null)
            Destroy(trainFront.gameObject);
        if(trainBack != null)
            Destroy(trainBack.gameObject);

        if (SceneLoader.s.currentLevel != null) {
            cartCount = SceneLoader.s.currentLevel.trainLength;
            LevelReferences.s.carts = new Cart[cartCount];

            var startPlace = transform.position + Vector3.back * DataHolder.s.cartLength * cartCount / 2f;
            //var startPlace = transform.position;
            for (int i = 0; i < cartCount; i++) {
                var cart = Instantiate(
                    DataHolder.s.cartPrefab,
                    startPlace + Vector3.forward * i * DataHolder.s.cartLength,
                    Quaternion.identity,
                    transform
                );
                var index = cartCount - i - 1; // because we start counting from back
                cart.name = $"Cart {index }";
                var cartScript = cart.GetComponent<Cart>();
                cartScript.index = index ;
                LevelReferences.s.carts[index] = cartScript;
                /*carts.Add(cart.transform);
                cartDefPositions.Add(cart.transform.position);*/
            }

            trainFront = new GameObject().transform;
            trainFront.SetParent(transform);
            trainFront.gameObject.name = "Train Front";
            trainFront.transform.position = transform.GetChild(transform.childCount - 1).position + trainFrontOffset;
            
            trainBack = new GameObject().transform;
            trainBack.SetParent(transform);
            trainBack.gameObject.name = "Train Back";
            trainBack.transform.position = transform.GetChild(0).position + trainFrontOffset;
        }
        
        /*carts.Reverse();
        cartDefPositions.Reverse();*/
    }


    /*[Header("Train Shake Settings")] 
    public Vector2 shakeDistance = new Vector2(1, 3);
    public Vector3 shakeOffsetMax = new Vector3(0.01f, 0.02f, 0.005f);

    private float curDistance = 0.1f;
    private void Update() {
        if (curDistance < 0) {
            StartCoroutine(ShakeWave());
            curDistance += Random.Range(shakeDistance.x, shakeDistance.y);
        } else {
            curDistance -= Time.deltaTime;
        }
    }

    IEnumerator ShakeWave() {
        var curShakePos = 0f;

        var cartCount = carts.Count;
        var cartLength = DataHolder.s.cartLength;
        var lastCart = -1;
        while (curShakePos < cartCount*cartLength) {
            var curCart = Mathf.FloorToInt(curShakePos / cartLength);
            curCart = Mathf.Clamp(curCart, 0, cartCount - 1);

            if (curCart != lastCart) {
                if(lastCart >= 0)
                    carts[lastCart].position = cartDefPositions[lastCart];

                carts[curCart].position = cartDefPositions[curCart] + new Vector3(
                    Random.Range(-shakeOffsetMax.x, shakeOffsetMax.x),
                    Random.Range(-shakeOffsetMax.y, shakeOffsetMax.y),
                    Random.Range(-shakeOffsetMax.z, shakeOffsetMax.z)
                );
                
                lastCart = curCart;
            }

            curShakePos += LevelReferences.s.speed * Time.deltaTime;
            yield return null;
        }
        
        carts[lastCart].position = cartDefPositions[lastCart];
    }*/
}
