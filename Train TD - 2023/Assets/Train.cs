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

    public List<Cart> carts = new List<Cart>();
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

            for (int i = 0; i < carts.Count; i++) {
                trainWeight += carts[i].weight;
            }

            var cargoComponents = GetComponentsInChildren<CargoModule>();
            cargoCount = cargoComponents.Length;

            trainWeightDirty = false;
            return trainWeight;
        }
    }


    private void Awake() {
        s = this;
    }


    private void Start() {
        //UpdateBasedOnLevelData();
        LevelReferences.s.train = this;
    }

    [Button]
    public void ReDrawCurrentTrain() {
        DrawTrain(GetTrainState());
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

        carts = new List<Cart>();
        cartDefPositions = new List<Vector3>();

        if (trainFront != null)
            Destroy(trainFront.gameObject);
        if (trainBack != null)
            Destroy(trainBack.gameObject);

        if (trainState != null) {
            cartCount = trainState.myCarts.Count;

            for (int i = 0; i < cartCount; i++) {
                var cartState = trainState.myCarts[i];
                var cart = Instantiate(DataHolder.s.GetCart(cartState.uniqueName).gameObject, transform).GetComponent<Cart>();
                ApplyStateToCart(cart, cartState);
                carts.Add(cart);
                cartDefPositions.Add(cart.transform.localPosition);
            }

            trainFront = new GameObject().transform;
            trainFront.SetParent(transform);
            trainFront.gameObject.name = "Train Front";

            trainBack = new GameObject().transform;
            trainBack.SetParent(transform);
            trainBack.gameObject.name = "Train Back";
        }

        UpdateCartPositions();

        for (int i = 0; i < carts.Count; i++) {
            carts[i].SetAttachedToTrainModulesMode(true);
        }

        trainUpdatedThroughNonBuildingActions?.Invoke();
        trainUpdated?.Invoke();

        isTrainDrawn = true;
    }


    public void SaveTrainState() {
        if (!PlayStateMaster.s.isCombatInProgress()) {
            Invoke(nameof(OneFrameLater), 0.01f);
        }
    }

    void OneFrameLater() {
        // because sometimes train doesnt get updated fast enough
        DataSaver.s.GetCurrentSave().currentRun.myTrain = GetTrainState();
        DataSaver.s.SaveActiveGame();
    }

    public DataSaver.TrainState GetTrainState() {
        var trainState = new DataSaver.TrainState();

        for (int i = 0; i < carts.Count; i++) {
            var cartScript = carts[i].GetComponent<Cart>();
            trainState.myCarts.Add(GetStateFromCart(cartScript));
        }

        return trainState;
    }

    public static void ApplyCartToState(Cart cart, DataSaver.TrainState.CartState buildingState) {
        if (cart != null) {
            buildingState.uniqueName = cart.uniqueName;
            //buildingState.health = cart.GetCurrentHealth();

            var cargo = cart.GetComponentInChildren<CargoModule>();
            if (cargo != null) {
                buildingState.cargoState = new DataSaver.TrainState.CartState.CargoState();
                buildingState.cargoState.cargoReward = cargo.GetReward();
                buildingState.cargoState.isLeftCargo = cargo.isLeftCargo;
            } else {
                buildingState.cargoState = null;
            }

            /*var ammo = cart.GetComponentInChildren<ModuleAmmo>();
            
            if (ammo != null) {
                buildingState.ammo = (int)ammo.curAmmo;
            } else {
                buildingState.ammo = -1;
            }*/
        } else {
            buildingState.EmptyState();
        }
    }

    public static DataSaver.TrainState.CartState GetStateFromCart(Cart cart) {
        var state = new DataSaver.TrainState.CartState();
        ApplyCartToState(cart, state);
        return state;
    }

    public static void ApplyStateToCart(Cart cart, DataSaver.TrainState.CartState cartState) {
        /*if (cartState.health > 0) {
            cart.SetCurrentHealth(cartState.health);
        }*/

        /*if (cartState.ammo >= 0) {
            var ammo = cart.GetComponentInChildren<ModuleAmmo>();
            ammo.SetAmmo(cartState.ammo);
        }else if (cartState.ammo == -2) {
            var ammo = cart.GetComponentInChildren<ModuleAmmo>();
            if (ammo != null) {
                ammo.SetAmmo(ammo.maxAmmo);
            } else {
                cartState.ammo = -1;
            }
        }*/

        var cargoModule = cart.GetComponentInChildren<CargoModule>();
        if (cargoModule != null) {
            cargoModule.SetCargo(cartState.cargoState);
        }
    }

    public void RightBeforeLeaveMissionRewardArea() {
        StopShake();
        StopCoroutine(nameof(LerpTrain));
        StartCoroutine(LerpTrain(Vector3.zero, Vector3.forward, 2f ,false));
        showEntryMovement = true;
    }

    public bool showEntryMovement = false;
    public void OnEnterShopArea() {
        StopShake();
        StopCoroutine(nameof(LerpTrain));
        if(showEntryMovement) // only show this if the player just left the mission reward area
            StartCoroutine(LerpTrain(Vector3.back,Vector3.zero, 2f , true));
    }


    IEnumerator LerpTrain(Vector3 startPos, Vector3 endPos, float time, bool stopSparkles) {
        transform.position = startPos;
        var totalTime = time;
        if (stopSparkles) {
            SpeedController.s.currentBreakPower = 10;
        }
        
        while (time >= 0f) {
            transform.position = Vector3.Lerp(endPos, startPos, Mathf.Pow(time/totalTime, 2));

            time -= Time.deltaTime;
            yield return null;
        }
        
        if (stopSparkles) {
            SpeedController.s.currentBreakPower = 0;
        }

        transform.position = endPos;
    }

    public void UpdateCartPositions() {
        if(carts.Count == 0)
            return;
        
        var totalLength = 0f;
        for (int i = 0; i < carts.Count; i++) {
            totalLength += carts[i].length;
        }
        
        var currentSpot = transform.localPosition - Vector3.back * (totalLength / 2f);

        if (cartDefPositions.Count != carts.Count) {
            cartDefPositions.Clear();
            for (int i = 0; i < carts.Count; i++) {
                cartDefPositions.Add(Vector3.zero);
            }
        }

        for (int i = 0; i < carts.Count; i++) {
            var cart = carts[i];
            cart.transform.localPosition = currentSpot;
            currentSpot += -Vector3.forward * cart.length;
            var index = i;
            cart.name = $"Cart {index }";
            cart.trainIndex = index;
            cartDefPositions[i] = cart.transform.localPosition;
        }
        
        trainFront.transform.localPosition = carts[carts.Count-1].transform.localPosition + trainFrontOffset;
        trainBack.transform.localPosition = carts[0].transform.localPosition + trainFrontOffset;
    }
    

    private bool suppressRedraw = false;
    public void CartDestroyed(Cart cart) {
        if(suppressRedraw)
            return;
        
        StopShake();

        var index = carts.IndexOf(cart);

        if (index > -1) {
            carts.Remove(cart);
            UpdateCartPositions();
        } /*else {
            Debug.Log($"Cart with illegal index {index} {cart} {cart.gameObject.name}");
        }*/
        
        RestartShake();

        var hasEngine = false;
        var hasCriticalComponent = false;

        for (int i = 0; i < carts.Count; i++) {
            if (carts[i].isMainEngine) {
                hasEngine = true;
            }

            if (carts[i].isCriticalComponent) {
                hasCriticalComponent = true;
            }
        }

        var lostGame = carts.Count <= 0 || !hasEngine || !hasCriticalComponent;

        if (lostGame && PlayStateMaster.s.isCombatInProgress()) {
            MissionLoseFinisher.s.MissionLost();
        }
        
        trainWeightDirty = true;
        
        // draw train already calls this
        //trainUpdatedThroughNonBuildingActions?.Invoke();
    }


    [Header("Train Shake Settings")] 
    public Vector2 shakeDistance = new Vector2(1, 3);
    public Vector3 shakeOffsetMax = new Vector3(0.01f, 0.02f, 0.005f);

    private float curDistance = 0.1f;
    public float restoreDelay = 0.1f;

    public bool doShake = true;
    private float shakeBlock = 0f;

    private void Update() {
        if (PlayStateMaster.s.isCombatInProgress()) {
            if (doShake) {
                if (curDistance < 0) {
                    StartCoroutine(ShakeWave());
                    StartCoroutine(RestoreWave(restoreDelay));
                    curDistance += Random.Range(shakeDistance.x, shakeDistance.y);
                } else {
                    curDistance -= LevelReferences.s.speed * Time.deltaTime;
                }
            } else {
                shakeBlock -= Time.deltaTime;
                if (shakeBlock <= 0) {
                    _RestartShake();
                }
            }
            
        } else {
            doShake = false;
        }
    }

    public void TrainCleanupForCombat() {
        LevelReferences.s.cartHealthParent.DeleteAllChildren();
        for (int i = 0; i < carts.Count; i++) {
            carts[i].transform.SetSiblingIndex((carts.Count - 1) - i);
        }
        
        for (int i = 0; i < carts.Count; i++) {
            carts[i].GetHealthModule()?.InitializeUIBar();
        }
    }


    public void RemoveCart(Cart cart) {
        trainWeightDirty = true;
        
        for (int i = 0; i < carts.Count; i++) {
            carts[i].SetAttachedToTrainModulesMode(false);
        }
        
        carts.Remove(cart);
        cart.myLocation = UpgradesController.CartLocation.world;
        cart.transform.SetParent(null);
        
        for (int i = 0; i < carts.Count; i++) {
            carts[i].trainIndex = i;
        }
        
        for (int i = 0; i < carts.Count; i++) {
            carts[i].SetAttachedToTrainModulesMode(true);
        }
    }

    public void AddCartAtIndex(int index, Cart cart) {
        trainWeightDirty = true;
        
        var wasShaking = doShake;
        if (wasShaking) {
            StopShake();
        }
        
        for (int i = 0; i < carts.Count; i++) {
            carts[i].SetAttachedToTrainModulesMode(false);
        }
        
        carts.Insert(index, cart);
        cart.myLocation = UpgradesController.CartLocation.train;
        cart.transform.SetParent(transform);

        for (int i = 0; i < carts.Count; i++) {
            carts[i].trainIndex = i;
        }
        
        for (int i = 0; i < carts.Count; i++) {
            carts[i].SetAttachedToTrainModulesMode(true);
        }

        if (wasShaking) {
            RestartShake();
        }
    }

    public void StopShake() {
        if (doShake) {
            StopCoroutine(nameof(_RestartShake));
            for (int i = 0; i < carts.Count; i++) {
                carts[i].transform.localPosition = cartDefPositions[i];
            }
            doShake = false;
        }
    }

    public void RestartShake() {
        if(shakeBlock < 0f)
            Invoke(nameof(_RestartShake), 0.01f); // one frame later so that any transform changes have been applied
    }

    public void SwapCarts(Cart cart1, Cart cart2) {
        var cart1Index = carts.IndexOf(cart1);
        var cart2Index = carts.IndexOf(cart2);

        if (cart1Index > cart2Index) {
            SwapCarts(cart2, cart1);
            return;
        }
        
        StopShake();
        
        RemoveCart(cart1);
        RemoveCart(cart2);

        (cart1.transform.position, cart2.transform.position) = (cart2.transform.position, cart1.transform.position);
        
        AddCartAtIndex(cart2Index, cart1);
        AddCartAtIndex(cart1Index, cart2);

        RestartShake();
    }

    void _RestartShake() {
        if (!doShake) {
            UpdateCartPositions();
            
            for (int i = 0; i < carts.Count; i++) {
                cartDefPositions[i] = carts[i].transform.localPosition;
            }

            doShake = true;
        }
    }


    IEnumerator ShakeWave() {
        if (PlayStateMaster.s.isCombatInProgress()) {
            var curShakePos = 0f;

            var cartCount = carts.Count;
            var cartLength = DataHolder.s.cartLength;
            var lastCart = -1;
            while (curShakePos < cartCount * cartLength) {
                var curCart = Mathf.FloorToInt(curShakePos / cartLength);
                curCart = Mathf.Clamp(curCart, 0, cartCount - 1);

                if (curCart != lastCart) {
                    carts[curCart].transform.localPosition = cartDefPositions[curCart] + new Vector3(
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
        if (PlayStateMaster.s.isCombatInProgress()) {
            yield return new WaitForSeconds(delay);
            
            var curShakePos = 0f;

            var cartCount = carts.Count;
            var cartLength = DataHolder.s.cartLength;
            var lastCart = -1;
            while (curShakePos < cartCount * cartLength) {
                var curCart = Mathf.FloorToInt(curShakePos / cartLength);
                curCart = Mathf.Clamp(curCart, 0, cartCount - 1);

                if (curCart != lastCart) {
                    carts[curCart].transform.localPosition = cartDefPositions[curCart];
                }

                curShakePos += LevelReferences.s.speed * Time.deltaTime;
                yield return null;
            }
        }
    }
    
    
    IEnumerator ShakeAndRestoreWave() {
        if (PlayStateMaster.s.isCombatInProgress()) {
            var curShakePos = 0f;

            var cartCount = carts.Count;
            var cartLength = DataHolder.s.cartLength;
            var lastCart = -1;
            while (curShakePos < cartCount * cartLength) {
                var curCart = Mathf.FloorToInt(curShakePos / cartLength);
                curCart = Mathf.Clamp(curCart, 0, cartCount - 1);

                if (curCart != lastCart) {
                    if (lastCart >= 0)
                        carts[lastCart].transform.localPosition = cartDefPositions[lastCart];

                    carts[curCart].transform.localPosition = cartDefPositions[curCart] + new Vector3(
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
                carts[lastCart].transform.localPosition = cartDefPositions[lastCart];
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
    

    public void UpdateTrainCartsBasedOnRotation(float rotationStartZ, float rotationEndZ, float maxXOffset, float arcLength, bool isGoingLeft) {// rotation angle is 45 degrees
        StopShake();
        shakeBlock = 1f;

        if (rotationStartZ > 0) { // before we start rotating the ground
            for (int i = 0; i < carts.Count; i++) {
                var curCart = carts[i];

                if (curCart.transform.position.z > rotationEndZ) { // pass the curve in the flat area
                    var pos = curCart.transform.position;
                    var posDelta = (pos.z - rotationEndZ);
                    pos.x = 1-Mathf.Cos(45 * Mathf.Deg2Rad);
                    pos.x *= maxXOffset;
                    pos.x += posDelta;
                    pos.x *= (isGoingLeft ? -1 : 1);
                    
                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.Euler(0,45*(isGoingLeft ? -1 : 1),0);

                }else if (curCart.transform.position.z > rotationStartZ) { // in the curve
                    var pos = curCart.transform.position;
                    var posDelta = (pos.z - rotationStartZ)/arcLength;

                    var angle = posDelta * 45f;
                    pos.x = 1-Mathf.Cos(angle * Mathf.Deg2Rad);
                    pos.x *= maxXOffset;
                    pos.x *= (isGoingLeft ? -1 : 1);

                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.Euler(0,angle*(isGoingLeft ? -1 : 1),0);
                } // rest of the carts are flat
            }
        }else if (rotationEndZ > 0) { // before we stop rotating
            for (int i = 0; i < carts.Count; i++) {
                var curCart = carts[i];
                
                var pos = curCart.transform.position;
                var angleDelta = Mathf.Clamp(pos.z, rotationStartZ, rotationEndZ);
                var angle = (angleDelta/arcLength) * 45f;
                var absAngle = Mathf.Abs(angle);

                if (pos.z > rotationEndZ) { // pass the curve in the flat area
                    var posDelta = (pos.z - rotationEndZ);
                    
                    pos.x = 1-Mathf.Cos(absAngle * Mathf.Deg2Rad);
                    pos.x *= maxXOffset;
                    pos.x += posDelta * (Mathf.Sin(absAngle * Mathf.Deg2Rad)/Mathf.Sin((90-absAngle)*Mathf.Deg2Rad));
                    pos.x *= (isGoingLeft ? -1 : 1);
                    
                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.Euler(0,angle*(isGoingLeft ? -1 : 1),0);

                }else if (pos.z > rotationStartZ) { // in the curve
                    
                    pos.x = Mathf.Abs(1-Mathf.Cos(absAngle * Mathf.Deg2Rad));
                    pos.x *= maxXOffset;
                    pos.x *= (isGoingLeft ? -1 : 1);
                    
                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.Euler(0,angle*(isGoingLeft ? -1 : 1),0);
                } else { // behind the curve
                    
                    var posDelta = (rotationStartZ - pos.z);
                    
                    pos.x = 1-Mathf.Cos(absAngle * Mathf.Deg2Rad);
                    pos.x *= maxXOffset;
                    pos.x += posDelta * (Mathf.Sin(absAngle * Mathf.Deg2Rad)/Mathf.Sin((90-absAngle)*Mathf.Deg2Rad));
                    pos.x *= (isGoingLeft ? -1 : 1);
                    
                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.Euler(0,angle*(isGoingLeft ? -1 : 1),0);
                }
            }
            
        } else { // after we stop rotating
            for (int i = 0; i < carts.Count; i++) {
                var curCart = carts[i];

                if (curCart.transform.position.z < rotationStartZ) { // pass the curve in the flat area in the waaay back
                    var pos = curCart.transform.position;
                    var posDelta = (pos.z - rotationEndZ);
                    pos.x = 1-Mathf.Cos(45 * Mathf.Deg2Rad);
                    pos.x *= maxXOffset;
                    pos.x += posDelta;
                    pos.x *= (isGoingLeft ? -1 : 1);
                    
                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.Euler(0,45*(isGoingLeft ? -1 : 1),0);

                }else if (curCart.transform.position.z < rotationEndZ) { // in the curve
                    var pos = curCart.transform.position;
                    var posDelta = (pos.z - rotationEndZ)/arcLength;

                    var angle = posDelta * 45f;
                    pos.x = 1-Mathf.Cos(angle * Mathf.Deg2Rad);
                    pos.x *= maxXOffset;
                    pos.x *= (isGoingLeft ? -1 : 1);

                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.Euler(0,angle*(isGoingLeft ? -1 : 1),0);
                } else {// rest of the carts are flat
                    var pos = curCart.transform.position;
                    pos.x = 0;

                    curCart.transform.position = pos;
                    curCart.transform.rotation = Quaternion.identity;
                } 
            }
        }

    }
    
    
    public Cart GetNextBuilding(bool isForward, Cart cart) {
        if (isForward) {
            var nextCart = cart.trainIndex - 1;
            if (nextCart >= 0) {
                return carts[nextCart];
            } else {
                return null;
            }
        } else {
            var nextCart = cart.trainIndex + 1;
            if (nextCart < Train.s.carts.Count) {
                return carts[nextCart];
            } else {
                return null;
            }
        }
    }
}

public abstract class ActivateWhenAttachedToTrain : MonoBehaviour {

    public GameObject attachmentThing;
    public List<GameObject> attachmentThings = new List<GameObject>();

    public bool isAttached = false;

    public void AttachedToTrain() {
        if (isAttached == false) {
            isAttached = true;
            
            _AttachedToTrain();
        }
    }

    protected abstract void _AttachedToTrain();

    protected abstract bool CanApply(Cart target);

    protected void ApplyBoost(Cart target, bool doApply) {
        if(target == null)
            return;
        if (CanApply(target)) {
            if (doApply) {
                attachmentThings.Add(
                    Instantiate(attachmentThing).GetComponent<AttachmentThingScript>().SetUp(GetComponentInParent<Cart>(), target)
                );
                _ApplyBoost(target, doApply);
            } else {
                _ApplyBoost(target, doApply);
            }
        }
    }

    protected abstract void _ApplyBoost(Cart target, bool doApply);

    public void DetachedFromTrain() {
        if (isAttached == true) {
            isAttached = false;
			
            DeleteAllAttachments();

            _DetachedFromTrain();
        }
    }
    
    
    protected abstract void _DetachedFromTrain();
	
    void DeleteAllAttachments() {
        for (int i = 0; i < attachmentThings.Count; i++) {
            if (attachmentThings[i] != null) {
                Destroy(attachmentThings[i]);
            }
        }
        
        attachmentThings.Clear();
    }

    private void OnDestroy() {
        DeleteAllAttachments();
    }
}
