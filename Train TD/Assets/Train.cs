using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Train : MonoBehaviour {
    public static Train s;

    public Transform trainFront;
    public Transform trainBack;
    public Vector3 trainFrontOffset;

    public int cartCount;
    
    public List<Transform> carts = new List<Transform>();
    public List<Vector3> cartDefPositions = new List<Vector3>();

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
        //UpdateBasedOnLevelData();
        LevelReferences.s.train = this;
    }

    public void UpdateBasedOnLevelData() {
        var childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }

        carts = new List<Transform>();
        cartDefPositions = new List<Vector3>();
        
        if(trainFront != null)
            Destroy(trainFront.gameObject);
        if(trainBack != null)
            Destroy(trainBack.gameObject);

        if (SceneLoader.s.currentLevel != null) {
            cartCount = SceneLoader.s.currentLevel.trainLength;

            for (int i = 0; i < cartCount; i++) {
                var cart = Instantiate(DataHolder.s.cartPrefab, transform);
                carts.Add(cart.transform);
                cartDefPositions.Add(cart.transform.position);
            }

            trainFront = new GameObject().transform;
            trainFront.SetParent(transform);
            trainFront.gameObject.name = "Train Front";
            
            trainBack = new GameObject().transform;
            trainBack.SetParent(transform);
            trainBack.gameObject.name = "Train Back";
        }
        
        carts.Reverse();
        cartDefPositions.Reverse();
        
        UpdateCartPositions();
    }

    public Transform AddTrainCartAtIndex(int index) {
        cartCount += 1;
        var cart = Instantiate(DataHolder.s.cartPrefab, transform);
        
        cart.name = $"Cart {index }";
        carts.Insert(index, cart.transform);
        cartDefPositions.Add(cart.transform.position);
        
        UpdateCartPositions();

        return cart.transform;
    }

    void UpdateCartPositions() {
        carts.Reverse();
        cartDefPositions.Reverse();
        var startPlace = transform.position + Vector3.back * DataHolder.s.cartLength * cartCount / 2f;

        for (int i = 0; i < carts.Count; i++) {
            var cart = carts[i];
            
            cart.position = startPlace + Vector3.forward * i * DataHolder.s.cartLength;
            var index = carts.Count - i - 1; // because we start counting from back
            cart.name = $"Cart {index }";
            var cartScript = cart.GetComponent<Cart>();
            cartScript.index = index ;
            cartDefPositions[i] = cart.transform.position;
        }
        
        
        trainFront.transform.position = carts[carts.Count-1].position + trainFrontOffset;
        trainBack.transform.position = carts[0].position + trainFrontOffset;
        
        
        carts.Reverse();
        cartDefPositions.Reverse();
    }


    [Header("Train Shake Settings")] 
    public Vector2 shakeDistance = new Vector2(1, 3);
    public Vector3 shakeOffsetMax = new Vector3(0.01f, 0.02f, 0.005f);

    private float curDistance = 0.1f;
    public float restoreDelay = 0.1f;
    private void Update() {
        if (curDistance < 0) {
            StartCoroutine(ShakeWave());
            StartCoroutine(RestoreWave(restoreDelay));
            curDistance += Random.Range(shakeDistance.x, shakeDistance.y);
        } else {
            curDistance -= LevelReferences.s.speed * Time.deltaTime;
        }
    }
    
    

    IEnumerator ShakeWave() {
        if (SceneLoader.s.isLevelInProgress) {
            var curShakePos = 0f;

            var cartCount = carts.Count;
            var cartLength = DataHolder.s.cartLength;
            var lastCart = -1;
            while (curShakePos < cartCount * cartLength) {
                var curCart = Mathf.FloorToInt(curShakePos / cartLength);
                curCart = Mathf.Clamp(curCart, 0, cartCount - 1);

                if (curCart != lastCart) {
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
        }
    }
    
    IEnumerator RestoreWave(float delay) {
        if (SceneLoader.s.isLevelInProgress) {
            yield return new WaitForSeconds(delay);
            
            var curShakePos = 0f;

            var cartCount = carts.Count;
            var cartLength = DataHolder.s.cartLength;
            var lastCart = -1;
            while (curShakePos < cartCount * cartLength) {
                var curCart = Mathf.FloorToInt(curShakePos / cartLength);
                curCart = Mathf.Clamp(curCart, 0, cartCount - 1);

                if (curCart != lastCart) {
                    carts[curCart].position = cartDefPositions[curCart];
                }

                curShakePos += LevelReferences.s.speed * Time.deltaTime;
                yield return null;
            }
        }
    }
    
    
    IEnumerator ShakeAndRestoreWave() {
        if (SceneLoader.s.isLevelInProgress) {
            var curShakePos = 0f;

            var cartCount = carts.Count;
            var cartLength = DataHolder.s.cartLength;
            var lastCart = -1;
            while (curShakePos < cartCount * cartLength) {
                var curCart = Mathf.FloorToInt(curShakePos / cartLength);
                curCart = Mathf.Clamp(curCart, 0, cartCount - 1);

                if (curCart != lastCart) {
                    if (lastCart >= 0)
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

            if (lastCart != -1) {
                carts[lastCart].position = cartDefPositions[lastCart];
            }
        }
    }
}
