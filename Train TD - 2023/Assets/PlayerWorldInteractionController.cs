using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.InputSystem;



public class PlayerWorldInteractionController : MonoBehaviour { 
    public static PlayerWorldInteractionController s;
    private void Awake() {
        s = this;
    }

    private void OnDestroy() {
        s = null;
    }
    
    public Cart selectedCart;
    public Vector3 cartBasePos;

    //public InputActionProperty dragCart;
    public InputActionReference showDetailClick;

    public InputActionReference clickCart;
    
    
    public InputActionReference clickGate;
    public InputActionReference moveCart;
    public InputActionReference repairCart;
    public InputActionReference reloadCart;
    public InputActionReference directControlCart;
    public InputActionReference engineBoost;


    public bool canSelect = true;
    
    protected void OnEnable()
    {
        //dragCart.action.Enable();
        clickCart.action.Enable();

        clickGate.action.Enable();
        moveCart.action.Enable();
        repairCart.action.Enable();
        reloadCart.action.Enable();
        directControlCart.action.Enable();
        engineBoost.action.Enable();
        
        showDetailClick.action.Enable();
    }

    

    protected void OnDisable()
    {
        //dragCart.action.Disable();
        clickCart.action.Disable();
        
        clickGate.action.Disable();
        moveCart.action.Disable();
        repairCart.action.Disable();
        reloadCart.action.Disable();
        directControlCart.action.Disable();
        engineBoost.action.Disable();
        
        showDetailClick.action.Disable();
    }

    public Color cantActColor = Color.white;
    public Color moveColor = Color.blue;
    public Color repairColor = Color.green;
    public Color reloadColor = Color.yellow;
    public Color directControlColor = Color.magenta;
    public Color engineBoostColor = Color.red;

    private void Update() {
        if (!canSelect || Pauser.s.isPaused || PlayStateMaster.s.isLoading) {
            if (selectedCart != null)
                SelectBuilding(selectedCart, false);

            return;
        }

        if (DirectControlMaster.s.directControlInProgress) {
            if (selectedCart != null)
                SelectBuilding(selectedCart, false);
            return;
        }

        if (!isDragStarted && !infoCard.isActiveAndEnabled) {
            CastRayToOutlineCart();
        }

        if (PlayStateMaster.s.isCombatInProgress()) {
            CheckAndDoClick();
        } else {
            CheckAndDoDrag();
            CheckGate();
        }
        
    }

    public void OnEnterCombat() {
        GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.clickGate);
    }

    public void OnEnterShopScreen() {
        SelectBuilding(null, false);
    }


    Vector2 GetMousePos() {
        return Mouse.current.position.ReadValue();
    }

    private GateScript lastGate;
    private bool clickedOnGate;
    void CheckGate() {
        RaycastHit hit;
        Ray ray = GetRay();
        if (Physics.Raycast(ray, out hit, 100f, LevelReferences.s.gateMask)) {
            var gate = hit.collider.GetComponentInParent<GateScript>();

            if (gate != lastGate) {
                lastGate = gate;
                lastGate._OnMouseEnter();
                OnSelectGate?.Invoke(lastGate, true);
                GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.clickGate);
            }

            if (clickCart.action.WasPressedThisFrame() || clickGate.action.WasPressedThisFrame()) {
                HideBuildingInfo();
                clickedOnGate = true;
            }

            if (clickedOnGate && (clickCart.action.WasReleasedThisFrame() || clickGate.action.WasReleasedThisFrame())) {
                lastGate._OnMouseUpAsButton();
            }
            
        } else {
            if (lastGate != null) {
                lastGate._OnMouseExit();
                OnSelectGate?.Invoke(lastGate, false);
                GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.clickGate);
                
                lastGate = null;
                
                clickedOnGate = false;
            }
        }
    }

    bool CanDragCart(Cart cart) {
        return !cart.isMainEngine && cart.canPlayerDrag;
    }

    private Vector2 dragStartPos;
    public bool isDragStarted = false;
    public bool isSnapping = false;
    public Vector3 offset;
    public SnapCartLocation sourceSnapLocation;
    void CheckAndDoDrag() {
        if (selectedCart != null) {
            if (clickCart.action.WasPressedThisFrame() || moveCart.action.WasPressedThisFrame()) {
                HideBuildingInfo();
                if (CanDragCart(selectedCart)) {
                    currentSnapLoc = null;
                    isSnapping = false;
                    sourceSnapLocation = selectedCart.GetComponentInParent<SnapCartLocation>();
                    prevCartTrainSnapIndex = -1;
                    selectedCart.transform.SetParent(null);
                    selectedCart.GetComponent<Rigidbody>().isKinematic = true;
                    selectedCart.GetComponent<Rigidbody>().useGravity = false;

                    dragStartPos = GetMousePos();
                    cartBasePos = selectedCart.transform.position;
                    offset = cartBasePos - GetMousePositionOnPlane();
                    isDragStarted = true;

                    if (Train.s.carts.Contains(selectedCart)) {
                        Train.s.RemoveCart(selectedCart);
                        UpgradesController.s.AddCartToShop(selectedCart, UpgradesController.CartLocation.world);
                    } else {
                        UpgradesController.s.ChangeCartLocation(selectedCart, UpgradesController.CartLocation.world);
                    }
                    
                    if (PlayStateMaster.s.isShop()) {
                        UpgradesController.s.UpdateCartShopHighlights();
                    } else {
                        UpgradesController.s.UpdateCargoHighlights();
                    }
                }
            }

            if (isDragStarted) {
                if (clickCart.action.IsPressed() || moveCart.action.IsPressed()) {

                    CheckIfSnapping();
                    if (!isSnapping) {
                        selectedCart.transform.position = GetMousePositionOnPlane() + offset;
                        selectedCart.transform.rotation = Quaternion.Slerp(selectedCart.transform.rotation, Quaternion.identity, slerpSpeed * Time.deltaTime);
                        offset = Vector3.Lerp(offset, Vector3.zero, lerpSpeed * Time.deltaTime);
                    } 

                } else {
                    isDragStarted = false;
                    if (!isSnapping) {
                        selectedCart.GetComponent<Rigidbody>().isKinematic = false;
                        selectedCart.GetComponent<Rigidbody>().useGravity = true;
                    }

                    if (PlayStateMaster.s.isShop()) {
                        UpgradesController.s.SnapDestinationCargos(selectedCart);
                        UpgradesController.s.UpdateCartShopHighlights();
                    } else {
                        UpgradesController.s.UpdateCargoHighlights();
                    }

                    if (selectedCart.isCriticalComponent && selectedCart.myLocation != UpgradesController.CartLocation.train) {
                        UpgradesController.s.RemoveCartFromShop(selectedCart);
                        Train.s.AddCartAtIndex(1, selectedCart);
                        selectedCart.GetComponent<Rigidbody>().isKinematic = true;
                        selectedCart.GetComponent<Rigidbody>().useGravity = false;
                    }


                    UpgradesController.s.SaveCartStateWithDelay();
                    Train.s.SaveTrainState();
                }
            }
            
        }
        
        UpdateTrainCartPositionsSlowly();
    }

    public SnapCartLocation currentSnapLoc;
    void CheckIfSnapping() {
        var carts = Train.s.carts;
        if (Mathf.Abs(GetMousePositionOnPlane().x) < 0.3f) {
            isSnapping = SnapToTrain();
        } 
        
        if(!isSnapping || Mathf.Abs(GetMousePositionOnPlane().x) > 0.3f){
            if (sourceSnapLocation != null && UpgradesController.s.WorldCartCount() <= 0) {
                if (sourceSnapLocation.snapTransform.childCount > 0 && prevCartTrainSnapIndex > 0) {
                    var prevCart = sourceSnapLocation.GetComponentInChildren<Cart>();
                    if (prevCart != null) {
                        UpgradesController.s.RemoveCartFromShop(prevCart);
                        Train.s.AddCartAtIndex(prevCartTrainSnapIndex, prevCart);
                    }
                }
            }
            prevCartTrainSnapIndex = -1;

            if (carts.Contains(selectedCart)) {
                Train.s.RemoveCart(selectedCart);
                UpgradesController.s.AddCartToShop(selectedCart, UpgradesController.CartLocation.world);
                isSnapping = false;
            }

            if (!selectedCart.isCargo || !PlayStateMaster.s.isShop()) { // we dont want cargo to snap to flea market locations
                RaycastHit hit;
                Ray ray = GetRay();

                if (Physics.Raycast(ray, out hit, 100f, LevelReferences.s.cartSnapLocationsLayer)) {
                    var snapLocation = hit.collider.gameObject.GetComponentInParent<SnapCartLocation>();

                    var snapLocationValidAndNew = snapLocation != null && snapLocation != currentSnapLoc;
                    var snapLocationCanAcceptCart = (!snapLocation.onlySnapCargo || selectedCart.isCargo) && !selectedCart.isCriticalComponent;
                    var snapLocationEmpty = snapLocation.snapTransform.childCount == 0;
                    var canSnap = snapLocationValidAndNew && snapLocationCanAcceptCart && snapLocationEmpty;

                    if (canSnap) {
                        isSnapping = true;
                        selectedCart.transform.SetParent(snapLocation.snapTransform);
                        currentSnapLoc = snapLocation;
                        UpgradesController.s.ChangeCartLocation(selectedCart, snapLocation.myLocation);
                        print("snapping to location");
                    }
                } else {
                    if (currentSnapLoc != null) {
                        isSnapping = false;
                        print("stopped snapping");
                        currentSnapLoc = null;
                        selectedCart.transform.SetParent(null);
                        UpgradesController.s.ChangeCartLocation(selectedCart, UpgradesController.CartLocation.world);
                    }
                }
            }
        }

        if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.UpdateCartShopHighlights();
        } else {
            UpgradesController.s.UpdateCargoHighlights();
        }
    }

    public int prevCartTrainSnapIndex = -1;
    private bool SnapToTrain() {
        var carts = Train.s.carts;
        var zPos = GetMousePositionOnPlane().z;

        var totalLength = 0f;
        for (int i = 0; i < carts.Count; i++) {
            totalLength += carts[i].length;
        }

        var currentSpot = transform.localPosition - Vector3.back * (totalLength / 2f);

        bool inserted = false;
        float distance = 0;

        var sourceSnapIsMarket = sourceSnapLocation != null && sourceSnapLocation.myLocation == UpgradesController.CartLocation.market;
        
        for (int i = 0; i < carts.Count; i++) {
            var cart = carts[i];
            currentSpot += -Vector3.forward * cart.length;

            if (i != 0 || sourceSnapIsMarket) {
                distance = Mathf.Abs((currentSpot.z + (cart.length / 2f)) - zPos);
                if (currentSpot.z + (cart.length / 2f) < zPos && distance < cart.length*4) {
                    if (cart != selectedCart) {
                        if (carts.Contains(selectedCart)) {
                            Train.s.RemoveCart(selectedCart);
                        } else {
                            UpgradesController.s.RemoveCartFromShop(selectedCart);
                        }

                        var canBeSwapped = true;
                        if (sourceSnapLocation != null && UpgradesController.s.WorldCartCount() <= 0) {
                            if (sourceSnapLocation.snapTransform.childCount > 0) {
                                var prevCart = sourceSnapLocation.GetComponentInChildren<Cart>();
                                if (prevCart != null) {
                                    UpgradesController.s.RemoveCartFromShop(prevCart);
                                    Train.s.AddCartAtIndex(prevCartTrainSnapIndex, prevCart);
                                }
                            }

                            if (sourceSnapIsMarket) {
                                var swapCart = Train.s.carts[i];
                                canBeSwapped = !swapCart.isCriticalComponent && !swapCart.isCargo && !swapCart.isMainEngine;
                                if (canBeSwapped) {
                                    Train.s.RemoveCart(swapCart);
                                    UpgradesController.s.AddCartToShop(swapCart, sourceSnapLocation.myLocation);
                                    swapCart.transform.SetParent(sourceSnapLocation.snapTransform);
                                    prevCartTrainSnapIndex = i;
                                } else {
                                    i += 1;
                                }
                            }
                        }

                        //if (canBeSwapped) {
                            inserted = true;
                            Train.s.AddCartAtIndex(i, selectedCart);
                        //}
                    } else {
                        inserted = true;
                    }

                    if(inserted)
                        return true;
                }
            }
        }

        if (!inserted) {
            if (carts[carts.Count - 1] != selectedCart && distance < carts[0].length*4) {
                if (carts.Contains(selectedCart)) {
                    Train.s.RemoveCart(selectedCart);
                } else {
                    UpgradesController.s.RemoveCartFromShop(selectedCart);
                }

                Train.s.AddCartAtIndex(carts.Count, selectedCart);
                return true;
            }
        }
        
        return false;
    }


    public float lerpSpeed = 5;
    public float slerpSpeed = 20;

    void UpdateTrainCartPositionsSlowly() {
        var carts = Train.s.carts;
        
        if(carts.Count == 0)
            return;
        
        var totalLength = 0f;
        for (int i = 0; i < carts.Count; i++) {
            totalLength += carts[i].length;
        }
        
        var currentSpot = transform.localPosition - Vector3.back * (totalLength / 2f);

        for (int i = 0; i < carts.Count; i++) {
            var cart = carts[i];
            if (cart == selectedCart && !cart.isMainEngine )
                currentSpot += Vector3.up * (isDragStarted ? 0.4f : 0.05f);


            cart.transform.localPosition = Vector3.Lerp(cart.transform.localPosition, currentSpot, lerpSpeed * Time.deltaTime);
            cart.transform.localRotation = Quaternion.Slerp(cart.transform.localRotation, Quaternion.identity, slerpSpeed * Time.deltaTime);

            if (cart == selectedCart && !cart.isMainEngine)
                currentSpot -= Vector3.up * (isDragStarted ? 0.4f : 0.05f);


            currentSpot += -Vector3.forward * cart.length;
            var index = i;
            cart.name = $"Cart {index}";
        }
    }

    public float repairAmountPerClick = 50f;
    public float reloadAmountPerClick = 2;

    void CheckAndDoClick() {
        if (selectedCart != null) {
            if (clickCart.action.WasPerformedThisFrame()) {
                HideBuildingInfo();
                switch (currentSelectMode) {
                    case SelectMode.cart:
                        selectedCart.GetHealthModule().Repair(repairAmountPerClick);
                        break;
                    case SelectMode.reload:
                        selectedCart.GetComponentInChildren<ModuleAmmo>().Reload(reloadAmountPerClick);
                        break;
                    case SelectMode.directControl:
                        DirectControlMaster.s.AssumeDirectControl(selectedCart.GetComponentInChildren<DirectControllable>());
                        break;
                    case SelectMode.engineBoost:
                        SpeedController.s.ActivateBoost();
                        break;
                }
            }
            
            if (repairCart.action.WasPerformedThisFrame()) {
                HideBuildingInfo();
                var health = selectedCart.GetHealthModule();
                if (health != null) {
                    health.Repair(repairAmountPerClick);
                }
            }

            if (reloadCart.action.WasPerformedThisFrame()) {
                HideBuildingInfo();
                var ammo = selectedCart.GetComponentInChildren<ModuleAmmo>();
                if (ammo != null) {
                    ammo.Reload(reloadAmountPerClick);
                }
            }
            
            if (directControlCart.action.WasPerformedThisFrame()) {
                HideBuildingInfo();
                var directControllable = selectedCart.GetComponentInChildren<DirectControllable>();
                if (directControllable != null) {
                    DirectControlMaster.s.AssumeDirectControl(directControllable);
                }
            }
            
            if (engineBoost.action.WasPerformedThisFrame()) {
                HideBuildingInfo();
                var engineBoostable = selectedCart.GetComponentInChildren<EngineBoostable>();
                if (engineBoostable != null) {
                    SpeedController.s.ActivateBoost();
                }
            }
        }
    }

    public void UIRepair(Cart cart) {
        cart.GetHealthModule()?.Repair(repairAmountPerClick);
    }

    public void UIReloadOrDirectControlOrBoost(Cart cart) {
        cart.GetComponentInChildren<ModuleAmmo>()?.Reload(reloadAmountPerClick);
        if (cart.GetComponentInChildren<DirectControllable>()) {
            DirectControlMaster.s.AssumeDirectControl(cart.GetComponentInChildren<DirectControllable>());
        }

        if (cart.GetComponentInChildren<EngineBoostable>()) {
            SpeedController.s.ActivateBoost();
        }
    }

    public Ray GetRay() {
        if (SettingsController.GamepadMode()) {
            return GamepadControlsHelper.s.GetRay();
        } else {
            return LevelReferences.s.mainCam.ScreenPointToRay(GetMousePos());
        }
    }

    public enum SelectMode {
        cart, reload, directControl, engineBoost, emptyCart
    }

    public SelectMode currentSelectMode = SelectMode.cart;

    public float sphereCastRadiusGamepad = 0.3f;
    public float sphereCastRadiusMouse = 0.1f;

    public float GetSphereCastRadius() {
        if (SettingsController.GamepadMode()) {
            return sphereCastRadiusGamepad;
        } else {
            return sphereCastRadiusMouse;
        }
    }

    public MiniGUI_BuildingInfoCard infoCard;
    void CastRayToOutlineCart() {
        RaycastHit hit;
        Ray ray = GetRay();

        if (Physics.SphereCast(ray, GetSphereCastRadius(), out hit, 100f, LevelReferences.s.buildingLayer)) {
            var lastSelectMode = currentSelectMode;
            currentSelectMode = SelectMode.emptyCart;
            
            if(hit.collider.GetComponentInParent<ModuleHealth>())
                currentSelectMode = SelectMode.cart;
            
            var directControllable = hit.collider.GetComponentInParent<DirectControllable>();
            if (SettingsController.GamepadMode() && directControllable == null) {
                directControllable = hit.rigidbody.gameObject.GetComponentInChildren<DirectControllable>();
            }
            if (directControllable != null) {
                currentSelectMode = SelectMode.directControl;
            }
            
            var reloadable = hit.collider.GetComponentInParent<Reloadable>();
            if (SettingsController.GamepadMode() && reloadable == null) {
                reloadable = hit.rigidbody.gameObject.GetComponentInChildren<Reloadable>();
            }
            if (reloadable != null) {
                currentSelectMode = SelectMode.reload;
            }
            
            var engineBoostable = hit.collider.GetComponentInParent<EngineBoostable>();
            if (SettingsController.GamepadMode() && engineBoostable == null) {
                engineBoostable = hit.rigidbody.gameObject.GetComponentInChildren<EngineBoostable>();
            }
            if (engineBoostable != null) {
                currentSelectMode = SelectMode.engineBoost;
            }
            
            var cart = hit.collider.GetComponentInParent<Cart>();

            if (cart.isDestroyed) {
                currentSelectMode = SelectMode.cart;
            }
            
            if (cart != selectedCart || lastSelectMode != currentSelectMode) {
                if (selectedCart != null)
                    SelectBuilding(selectedCart, false);
                
                selectedCart = cart;

                SelectBuilding(selectedCart, true);

            } else {
                if (PlayStateMaster.s.isShopOrEndGame() && showDetailClick.action.WasPerformedThisFrame() /*|| (holdOverTimer > infoShowTime && !SettingsController.GamepadMode())*/) {
                    ShowSelectedBuildingInfo();
                }
            }

        } else {
            if (selectedCart != null) {
                SelectBuilding(selectedCart, false);
                selectedCart = null;
            }
        }
    }
    
    

    void ShowSelectedBuildingInfo() {
        if (!infoCard.isActiveAndEnabled) {
            infoCard.SetUp(selectedCart);
        }
    }

    void HideBuildingInfo() {
        infoCard.Hide();
    }


    [HideInInspector]
    public UnityEvent<Cart, bool> OnSelectBuilding = new UnityEvent<Cart, bool>();
    public UnityEvent<GateScript, bool> OnSelectGate = new UnityEvent<GateScript, bool>();
    void SelectBuilding(Cart building, bool isSelecting) {
        HideBuildingInfo();
        Outline outline = null;
        if(building != null)
            outline = building.GetComponentInChildren<Outline>();

        if (isSelecting) {
            Color myColor = moveColor;

            if (PlayStateMaster.s.isShopOrEndGame()) {
                GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.showDetails);
                GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.move);
                if (!CanDragCart(building)) {
                    myColor = cantActColor;
                    GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.move);
                }
            }

            if (PlayStateMaster.s.isCombatInProgress()) {
                if(SettingsController.GamepadMode())
                    GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.repair);
                
                switch (currentSelectMode) {
                    case SelectMode.cart:
                        myColor = repairColor;
                        GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.repair);
                        break;
                    case SelectMode.reload:
                        myColor = reloadColor;
                        GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.reload);
                        break;
                    case SelectMode.directControl:
                        myColor = directControlColor;
                        GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.directControl);
                        break;
                    case SelectMode.engineBoost:
                        myColor = engineBoostColor;
                        GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.engineBoost);
                        break;
                    case SelectMode.emptyCart:
                        myColor = cantActColor;
                        break;
                }
            }

            outline.OutlineColor = myColor;
        } else {
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.move);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.repair);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.reload);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.directControl);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.showDetails);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.engineBoost);
        }
        

        if (building != null) {
            outline.enabled = isSelecting;
            var ranges = building.GetComponentsInChildren<RangeVisualizer>();
            for (int i = 0; i < ranges.Length; i++) {
                ranges[i].ChangeVisualizerEdgeShowState(isSelecting);
            }
        }

        OnSelectBuilding?.Invoke(building, isSelecting);
    }
    
    
    
    


    private void LogData(bool currentlyMultiBuilding, Cart newBuilding) {
        var buildingName = newBuilding.uniqueName;
        

        /*if (currentLevelStats.TryGetValue(buildingName, out BuildingData data)) {
            data.constructionData.Add(cData);
        } else {
            var toAdd = new BuildingData();
            toAdd.uniqueName = buildingName;
            currentLevelStats.Add(buildingName, toAdd);
        }*/
    }

    public void LogCurrentLevelBuilds(bool isWon) {
        /*foreach (var keyValPair in currentLevelStats) {
            var bName = keyValPair.Key;
            var bData = keyValPair.Value;
            var constStats = bData.constructionData;

            Dictionary<string, object> resultingDictionary = new Dictionary<string, object>();

            resultingDictionary["currentLevel"] = SceneLoader.s.currentLevel.levelName;
            resultingDictionary["isWon"] = isWon;

            resultingDictionary["buildCount"] = constStats.Count;

            if (constStats.Count > 0) {
                var averageTrainPosition = constStats.Average(x => x.buildTrainPercent);
                resultingDictionary["buildTrainPercent"] = RatioToStatsPercent(averageTrainPosition);

                var multiBuildRatio = (float)constStats.Count(x => x.isMultiBuild) / (float)constStats.Count;
                resultingDictionary["isMultiBuild"] = RatioToStatsPercent(multiBuildRatio);

                var averageBuildLevelDistance = constStats.Average(x => x.buildLevelDistance);
                resultingDictionary["buildMissionDistance"] = DistanceToStats(averageBuildLevelDistance);

                TrainBuilding.Rots maxRepeated = constStats.GroupBy(s => s.buildRotation)
                    .OrderByDescending(s => s.Count())
                    .First().Key;
                resultingDictionary["buildRotation"] = maxRepeated;
            } 
            

            resultingDictionary["buildDamage"] = (int)bData.damageData;

            //print(resultingDictionary);

            AnalyticsResult analyticsResult = Analytics.CustomEvent(
                bName,
                resultingDictionary
                );
            
            Debug.Log("Building Build Data Analytics " + analyticsResult);
            
            Instantiate(statsPrefab, statsParent).GetComponent<MiniGUI_StatDisplay>().SetUp(bName + " Build Count", (constStats.Count).ToString());
            Instantiate(statsPrefab, statsParent).GetComponent<MiniGUI_StatDisplay>().SetUp(bName + " Damage", ((int)bData.damageData).ToString());
        }*/
    }
    

    Vector3 GetMousePositionOnPlane() {
        Plane plane = new Plane(Vector3.up, new Vector3(0,0.5f,0));

        float distance;
        Ray ray = GetRay();
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        } else {
            return Vector3.zero;
        }
    }

}


