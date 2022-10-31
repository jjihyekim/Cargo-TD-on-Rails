using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
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

    public bool trainWeightDirty = false;
    private int trainWeight = 0;

    public UnityEvent trainUpdatedThroughNonBuildingActions = new UnityEvent();

    public int GetTrainWeight() {
        if (!trainWeightDirty) {
            return trainWeight;
        } else {
            trainWeight = 0;
            var cartComponents = GetComponentsInChildren<Cart>();
            var trainBuildingComponents = GetComponentsInChildren<TrainBuilding>();

            for (int i = 0; i < cartComponents.Length; i++) {
                trainWeight += cartComponents[i].weight;
            }
            for (int i = 0; i < trainBuildingComponents.Length; i++) {
                trainWeight += trainBuildingComponents[i].weight;
            }

            trainWeightDirty = false;
            return trainWeight;
        }
    }

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

    public void DrawTrain(DataSaver.TrainState trainState) {
        StopAllCoroutines();
        transform.DeleteAllChildren();
        trainWeightDirty = true;

        carts = new List<Transform>();
        cartDefPositions = new List<Vector3>();
        
        if(trainFront != null)
            Destroy(trainFront.gameObject);
        if(trainBack != null)
            Destroy(trainBack.gameObject);

        if (trainState != null) {
            cartCount = trainState.myCarts.Count;

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

        AddBuildingsToTrain(trainState);
        trainUpdatedThroughNonBuildingActions?.Invoke();
    }

    public DataSaver.TrainState GetTrainState() {
        var trainState = new DataSaver.TrainState();

        for (int i = 0; i < carts.Count; i++) {
            var cartScript = carts[i].GetComponent<Cart>();
            var cartState = new DataSaver.TrainState.CartState();

            for (int j = 0; j < cartScript.frontSlot.myBuildings.Length; j++) {
                var buildingState = cartState.buildingStates[j];
                var building = cartScript.frontSlot.myBuildings[j];
                ApplyBuildingToState(building, buildingState);
            }
            
            for (int j = 0; j < cartScript.backSlot.myBuildings.Length; j++) {
                var buildingState = cartState.buildingStates[j+4];
                var building = cartScript.backSlot.myBuildings[j];
                ApplyBuildingToState(building, buildingState);
            }

            trainState.myCarts.Add(cartState);
        }

        return trainState;
    }

    private static void ApplyBuildingToState(TrainBuilding building, DataSaver.TrainState.CartState.BuildingState buildingState) {
        if (building != null) {
            buildingState.uniqueName = building.uniqueName;
            buildingState.health = building.GetCurrentHealth();

            var cargo = building.GetComponent<CargoModule>();
            if (cargo != null) {
                buildingState.cargoCost = cargo.moneyCost;
                buildingState.cargoReward = cargo.moneyReward;
            } else {
                buildingState.cargoCost = -1;
                buildingState.cargoReward = -1;
            }

            var ammo = building.GetComponent<ModuleAmmo>();
            
            if (ammo != null) {
                buildingState.ammo = ammo.curAmmo;
            } else {
                buildingState.ammo = -1;
            }
        } else {
            buildingState.EmptyState();
        }
    }

    void AddBuildingsToTrain(DataSaver.TrainState trainState) {
        for (int i = 0; i < trainState.myCarts.Count; i++) {
            var cartState = trainState.myCarts[i];

            var cart = carts[i].GetComponent<Cart>();

            var skipCount = 0;
            for (int j = 0; j < cartState.buildingStates.Length; j++) {
                if (skipCount > 0) { // we skip the next three duplicates if the prev building was an entire slot building
                    skipCount -= 1;
                    continue;
                }

                if (cartState.buildingStates[j].uniqueName.Length > 0) {
                    var slot = j < 4 ? cart.frontSlot : cart.backSlot;
                    var buildingScript = DataHolder.s.GetBuilding(cartState.buildingStates[j].uniqueName);
                    if (buildingScript != null) {
                        AddBuildingToSlot(slot, j % 4,buildingScript, cartState.buildingStates[j]);
                        if (buildingScript.occupiesEntireSlot) {
                            skipCount = 3;
                        } else if (j%4 == 0) {//we skip the second top slot as it is just for forward/backward slot
                            skipCount = 1;
                        }
                    }
                }
            }
        }
    }

    private static void AddBuildingToSlot(Slot slot, int slotIndex, TrainBuilding buildingScript, DataSaver.TrainState.CartState.BuildingState buildingState) {
        var newBuilding = Instantiate(buildingScript.gameObject).GetComponent<TrainBuilding>();
        slot.AddBuilding(newBuilding, slotIndex);
        if (buildingState.health > 0) {
            newBuilding.SetCurrentHealth(buildingState.health);
        }

        if (buildingState.ammo >= 0) {
            newBuilding.GetComponent<ModuleAmmo>().SetAmmo(buildingState.ammo);
        }

        if (buildingState.cargoCost >= 0) {
            var cargo = newBuilding.GetComponent<CargoModule>();
            cargo.moneyCost = buildingState.cargoCost;
            cargo.moneyReward = buildingState.cargoReward;
        }

        newBuilding.CompleteBuilding(false);
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
        if(carts.Count == 0)
            return;
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

    public void CartDestroyed(Cart cart) {
        var index = carts.IndexOf(cart.transform);

        if (index > -1) {
            var state = GetTrainState();
            state.myCarts.RemoveAt(index);
            DrawTrain(state);
        }
        
        if(carts.Count <= 0 && SceneLoader.s.isLevelInProgress)
            MissionLoseFinisher.s.MissionLost();
        
        
        trainUpdatedThroughNonBuildingActions?.Invoke();
    }

    public UnityEvent onLevelStateChanged = new UnityEvent();
    
    public void LevelStateChanged() {
        onLevelStateChanged?.Invoke();
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

    public float GetTrainLength() {
        return Train.s.carts.Count *DataHolder.s.cartLength;
    }
}
