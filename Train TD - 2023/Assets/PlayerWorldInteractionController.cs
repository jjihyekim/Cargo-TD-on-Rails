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
    public InputActionReference clickCart;
    public InputActionReference showDetailClick;


    public bool canSelect = true;
    
    protected void OnEnable()
    {
        //dragCart.action.Enable();
        clickCart.action.Enable();
        showDetailClick.action.Enable();
    }

    

    protected void OnDisable()
    {
        //dragCart.action.Disable();
        clickCart.action.Disable();
        showDetailClick.action.Disable();
    }

    public Color cantActColor = Color.white;
    public Color moveColor = Color.blue;
    public Color repairColor = Color.green;
    public Color reloadColor = Color.yellow;
    public Color directControlColor = Color.magenta;

    private void Update() {
        if (!canSelect) {
            if (selectedCart != null)
                SelectBuilding(selectedCart, false);

            return;
        }

        if (DirectControlMaster.s.directControlInProgress) {
            if (selectedCart != null)
                SelectBuilding(selectedCart, false);
            return;
        }

        if (!isDragStarted) {
            CastRayToOutlineCart();
        }

        if (PlayStateMaster.s.isCombatInProgress()) {
            CheckAndDoClick();
        } else {
            CheckAndDoDrag();
            CheckGate();
        }
        
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
            }

            if (clickCart.action.WasPressedThisFrame()) {
                clickedOnGate = true;
            }

            if (clickedOnGate && clickCart.action.WasReleasedThisFrame()) {
                lastGate._OnMouseUpAsButton();
            }
            
        } else {
            if (lastGate != null) {
                lastGate._OnMouseExit();
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
            if (clickCart.action.WasPressedThisFrame()) {
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
                if (clickCart.action.IsPressed()) {

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
                    var snapLocationCanAcceptCart = !snapLocation.onlySnapCargo || selectedCart.isCargo;
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
        for (int i = 0; i < carts.Count; i++) {
            var cart = carts[i];
            currentSpot += -Vector3.forward * cart.length;

            if (i != 0) {
                distance = Mathf.Abs((currentSpot.z + (cart.length / 2f)) - zPos);
                if (currentSpot.z + (cart.length / 2f) < zPos && distance < cart.length*2) {
                    if (cart != selectedCart) {
                        if (carts.Contains(selectedCart)) {
                            Train.s.RemoveCart(selectedCart);
                        } else {
                            UpgradesController.s.RemoveCartFromShop(selectedCart);
                        }

                        if (sourceSnapLocation != null && UpgradesController.s.WorldCartCount() <= 0) {
                            if (sourceSnapLocation.snapTransform.childCount > 0) {
                                var prevCart = sourceSnapLocation.GetComponentInChildren<Cart>();
                                if (prevCart != null) {
                                    UpgradesController.s.RemoveCartFromShop(prevCart);
                                    Train.s.AddCartAtIndex(prevCartTrainSnapIndex, prevCart);
                                }
                            }

                            if (sourceSnapLocation.myLocation == UpgradesController.CartLocation.market) {
                                var swapCart = Train.s.carts[i];
                                Train.s.RemoveCart(swapCart);
                                UpgradesController.s.AddCartToShop(swapCart, sourceSnapLocation.myLocation);
                                swapCart.transform.SetParent(sourceSnapLocation.snapTransform);
                                prevCartTrainSnapIndex = i;
                            }
                        }

                        Train.s.AddCartAtIndex(i, selectedCart);
                    }

                    inserted = true;
                    return true;
                }
            }
        }

        if (!inserted) {
            if (carts[carts.Count - 1] != selectedCart && distance < carts[0].length*2) {
                if (carts.Contains(selectedCart)) {
                    Train.s.RemoveCart(selectedCart);
                } else {
                    UpgradesController.s.RemoveCartFromShop(selectedCart);
                }

                Train.s.AddCartAtIndex(carts.Count, selectedCart);
            }
            return true;
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
                }
                holdOverTimer = -1000;
            }
        }
    }

    public Ray GetRay() {
        if (SettingsController.GamepadMode()) {
            return GamepadControlsHelper.s.GetRay();
        } else {
            return LevelReferences.s.mainCam.ScreenPointToRay(GetMousePos());
        }
    }

    public float holdOverTimer;
    public float infoShowTime = 3f;

    public enum SelectMode {
        cart, reload, directControl
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
            currentSelectMode = SelectMode.cart;
            
            var directControllable = hit.collider.GetComponentInParent<DirectControllable>();
            if (directControllable != null) {
                currentSelectMode = SelectMode.directControl;
            }
            
            var reloadable = hit.collider.GetComponentInParent<Reloadable>();
            if (reloadable != null) {
                currentSelectMode = SelectMode.reload;
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

                holdOverTimer = 0;

            } else {
                holdOverTimer += Time.deltaTime;

                if (showDetailClick.action.WasPerformedThisFrame() || holdOverTimer > infoShowTime) {
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
        holdOverTimer = 0;
    }


    void SelectBuilding(Cart building, bool isSelecting) {
        HideBuildingInfo();
        var outline = building.GetComponentInChildren<Outline>();

        Color myColor = moveColor;
        if (PlayStateMaster.s.isShop()) {
            if (!CanDragCart(building))
                myColor = cantActColor;
        }
        
        if (PlayStateMaster.s.isCombatInProgress()) {
            switch (currentSelectMode) {
                case SelectMode.cart:
                    myColor = repairColor;
                    break;
                case SelectMode.reload:
                    myColor = reloadColor;
                    break;
                case SelectMode.directControl:
                    myColor = directControlColor;
                    break;
            }
        }

        outline.OutlineColor = myColor;
        outline.enabled = isSelecting;

        var ranges = building.GetComponentsInChildren<RangeVisualizer>();
        for (int i = 0; i < ranges.Length; i++) {
            ranges[i].ChangeVisualizerEdgeShowState(isSelecting);
        }
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


