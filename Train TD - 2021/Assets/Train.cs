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
    private int cargoCount = 0;

    public UnityEvent trainUpdatedThroughNonBuildingActions = new UnityEvent();
    public UnityEvent trainUpdated = new UnityEvent();

    public bool isTrainDrawn = false;
    public int GetCargoCount() {
        GetTrainWeight(); // so that it gets updated
        return cargoCount;
    }
    
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

            var cargoComponents = GetComponentsInChildren<CargoModule>();
            cargoCount = cargoComponents.Length;

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

    public void DrawTrainBasedOnSaveData() {
        DrawTrain(DataSaver.s.GetCurrentSave().currentRun.myTrain);
    }

    public void DrawTrain(DataSaver.TrainState trainState) {
        StopAllCoroutines();
        suppressRedraw = true;
        transform.DeleteAllChildren();
        suppressRedraw = false;
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
                cartDefPositions.Add(cart.transform.localPosition);
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
        ReCalculateStorageAmounts();
        trainUpdatedThroughNonBuildingActions?.Invoke();
        trainUpdated?.Invoke();

        isTrainDrawn = true;
    }


    public void SaveTrainState(bool forceDuringLevel = false) {
        if (!SceneLoader.s.isLevelInProgress) {
            Invoke(nameof(OneFrameLater), 0.01f);
        }
    }

    void OneFrameLater() { // because sometimes train doesnt get updated fast enough
        DataSaver.s.GetCurrentSave().currentRun.myTrain = GetTrainState();
        DataSaver.s.SaveActiveGame();
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

            /*var cargo = building.GetComponent<CargoModule>();
            if (cargo != null) {
                //buildingState.cargoCost = cargo.moneyCost;
                buildingState.cargoReward = cargo.moneyReward;
            } else {
                buildingState.cargoCost = -1;
                buildingState.cargoReward = -1;
            }*/

            var ammo = building.GetComponent<ModuleAmmo>();
            
            if (ammo != null) {
                buildingState.ammo = (int)ammo.curAmmo;
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
            var ammo = newBuilding.GetComponent<ModuleAmmo>();
            ammo.SetAmmo(buildingState.ammo);
        }else if (buildingState.ammo == -2) {
            var ammo = newBuilding.GetComponent<ModuleAmmo>();
            if (ammo != null) {
                ammo.SetAmmo(ammo.maxAmmo);
            } else {
                buildingState.ammo = -1;
            }
        }

        /*if (buildingState.cargoCost >= 0) {
            var cargo = newBuilding.GetComponent<CargoModule>();
            //cargo.moneyCost = buildingState.cargoCost;
            cargo.moneyReward = buildingState.cargoReward;
        }*/

        newBuilding.CompleteBuilding(false);
    }

    public Transform AddTrainCartAtIndex(int index) {
        cartCount += 1;
        var cart = Instantiate(DataHolder.s.cartPrefab, transform);
        
        cart.name = $"Cart {index }";
        carts.Insert(index, cart.transform);
        cartDefPositions.Add(cart.transform.localPosition);
        
        UpdateCartPositions();

        return cart.transform;
    }

    void UpdateCartPositions() {
        if(carts.Count == 0)
            return;
        carts.Reverse();
        cartDefPositions.Reverse();
        var startPlace = transform.localPosition + Vector3.back * DataHolder.s.cartLength * cartCount / 2f;

        for (int i = 0; i < carts.Count; i++) {
            var cart = carts[i];
            
            cart.localPosition = startPlace + Vector3.forward * i * DataHolder.s.cartLength;
            var index = carts.Count - i - 1; // because we start counting from back
            cart.name = $"Cart {index }";
            var cartScript = cart.GetComponent<Cart>();
            cartScript.index = index ;
            cartDefPositions[i] = cart.transform.localPosition;
        }
        
        trainFront.transform.localPosition = carts[carts.Count-1].localPosition + trainFrontOffset;
        trainBack.transform.localPosition = carts[0].localPosition + trainFrontOffset;
        
        
        carts.Reverse();
        cartDefPositions.Reverse();
    }

    private bool suppressRedraw = false;
    public void CartDestroyed(Cart cart) {
        if(suppressRedraw)
            return;

        var index = carts.IndexOf(cart.transform);

        if (index > -1) {
            var state = GetTrainState();
            state.myCarts.RemoveAt(index);
            DrawTrain(state);
        } /*else {
            Debug.Log($"Cart with illegal index {index} {cart} {cart.gameObject.name}");
        }*/
        
        if(carts.Count <= 0 && SceneLoader.s.isLevelInProgress)
            MissionLoseFinisher.s.MissionLost();
        
        
        // draw train already calls this
        //trainUpdatedThroughNonBuildingActions?.Invoke();
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

    public bool doShake = true;
    private void Update() {
        if (SceneLoader.s.isLevelInProgress && doShake) {
            if (curDistance < 0) {
                StartCoroutine(ShakeWave());
                StartCoroutine(RestoreWave(restoreDelay));
                curDistance += Random.Range(shakeDistance.x, shakeDistance.y);
            } else {
                curDistance -= LevelReferences.s.speed * Time.deltaTime;
            }
        }
    }

    public void StopShake() {
        StopAllCoroutines();
        for (int i = 0; i < carts.Count; i++) {
            carts[i].localPosition = cartDefPositions[i];
        }

        doShake = false;
    }

    public void RestartShake() {
        Invoke(nameof(_RestartShake), 0.01f); // one frame later so that any transform changes have been applied
    }

    public void SwapCarts(Cart cart1, Cart cart2) {
        StopShake();

        (cart1.transform.position, cart2.transform.position) = (cart2.transform.position, cart1.transform.position);

        var cart1Index = carts.IndexOf(cart1.transform);
        var cart2Index = carts.IndexOf(cart2.transform);

        carts[cart1Index] = cart2.transform;
        carts[cart2Index] = cart1.transform;

        RestartShake();
    }

    void _RestartShake() {
        for (int i = 0; i < carts.Count; i++) {
            cartDefPositions[i] =  carts[i].localPosition;
        }

        doShake = true;
    }


    public void ReCalculateStorageAmounts() {
        if (DataSaver.s.GetCurrentSave().isInARun) {
            var charMinAmounts = DataSaver.s.GetCurrentSave().currentRun.character.starterResources;
            var maxScraps = 0;
            var maxFuel = 0;
            var maxAmmo = 0;


            var modules = GetComponentsInChildren<ModuleStorage>();

            for (int i = 0; i < modules.Length; i++) {
                switch (modules[i].myType) {
                    case ResourceTypes.ammo:
                        maxAmmo += modules[i].amount;
                        break;
                    case ResourceTypes.fuel:
                        maxFuel += modules[i].amount;
                        break;
                    case ResourceTypes.scraps:
                        maxScraps += modules[i].amount;
                        break;
                }
            }

            maxScraps = Mathf.Max(maxScraps, charMinAmounts.maxScraps);
            maxFuel = Mathf.Max(maxFuel, charMinAmounts.maxFuel);
            maxAmmo = Mathf.Max(maxAmmo, charMinAmounts.maxAmmo);


            var mySave = DataSaver.s.GetCurrentSave().currentRun.myResources;

            mySave.maxFuel = maxFuel;
            mySave.maxScraps = maxScraps;
            mySave.maxAmmo = maxAmmo;

            MoneyController.s.ApplyStorageAmounts(maxScraps, maxAmmo,maxFuel);
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
                    carts[curCart].localPosition = cartDefPositions[curCart] + new Vector3(
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
                    carts[curCart].localPosition = cartDefPositions[curCart];
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
                        carts[lastCart].localPosition = cartDefPositions[lastCart];

                    carts[curCart].localPosition = cartDefPositions[curCart] + new Vector3(
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
                carts[lastCart].localPosition = cartDefPositions[lastCart];
            }
        }
    }

    public float GetTrainLength() {
        return Train.s.carts.Count *DataHolder.s.cartLength;
    }

    public void ResetTrainPosition() {
        transform.ResetTransformation();
    }

    public void TrainUpdated() {
        trainUpdated?.Invoke();
    }
}
