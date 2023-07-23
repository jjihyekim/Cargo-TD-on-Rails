using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerWorldInteractionController : MonoBehaviour { 
    public static PlayerWorldInteractionController s;
    private void Awake() {
        s = this;
    }

    private void OnDestroy() {
        s = null;
    }

    public EnemyHealth selectedEnemy;
    public Artifact selectedArtifact;
    
    public Cart selectedCart;
    public Vector3 dragBasePos;

    //public InputActionProperty dragCart;
    public InputActionReference showDetailClick;

    public InputActionReference clickCart;
    
    
    public InputActionReference clickGate;

    public enum CursorState {
        empty, repair, shieldUp, reload_basic, reload_fire, reload_sticky 
    }


    [SerializeField]
    private bool _canSelect;
    public bool canSelect {
        get {
            return _canSelect;
        }
        set {
            SetCannotSelect(value);
        }
}

    public bool canRepair = true;
    public bool autoRepairAtStation = true;
    public bool canReload = true;
    public bool autoReloadAtStation = true;
    public bool canSmith = true;

    public bool engineBoostDamageInstead = false;
    
    public void ResetValues() {
        repairAmountMultiplier = 1;
        reloadAmountMultiplier = 1;
        shieldUpAmountMultiplier = 1;
        canRepair = true;
        canReload = true;
        canSmith = true;
        autoRepairAtStation = true;
        autoReloadAtStation = true;
        engineBoostDamageInstead = false;
    }
    
    protected void OnEnable()
    {
        //dragCart.action.Enable();
        clickCart.action.Enable();

        clickGate.action.Enable();
        
        showDetailClick.action.Enable();
    }

    

    protected void OnDisable()
    {
        //dragCart.action.Disable();
        clickCart.action.Disable();
        
        clickGate.action.Disable();
        
        
        showDetailClick.action.Disable();
    }

    public Color cantActColor = Color.white;
    public Color moveColor = Color.blue;
    public Color repairColor = Color.green;
    public Color reloadColor = Color.yellow;
    public Color directControlColor = Color.magenta;
    public Color engineBoostColor = Color.red;

    private void Update() {
        if (!canSelect || Pauser.s.isPaused || PlayStateMaster.s.isLoading || DirectControlMaster.s.directControlInProgress) {
            return;
        }

        if (infoCardActive) {
            if (showDetailClick.action.WasPerformedThisFrame() || clickCart.action.WasPerformedThisFrame()) {
                HideInfo();
            }
        }

        if (!isDragStarted && !infoCardActive) {
            CastRayToOutlineCart();
            if(PlayStateMaster.s.isCombatInProgress())
                CastRayToOutlineEnemy();
            else
                CastRayToOutlineArtifact();
        }

        if (PlayStateMaster.s.isCombatInProgress()) {
            CheckAndDoClick();
        } else {
            CheckAndDoDrag();
            CheckGate();
        }
    }

    public void SetCannotSelect(bool __canSelect) {
        _canSelect = __canSelect;
        if (!_canSelect) {
            Deselect();
            cursorStateObject.gameObject.SetActive(false);
        } else {
            SetCursorState(currentCursorState, activeCursorStateColor);
        }
        
        
    }

    public void OnEnterCombat() {
        GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.clickGate);
        SetCursorState(CursorState.repair, repairColor);
    }

    public void OnLeaveCombat() {
        Deselect();
        SetCursorState(CursorState.empty, cantActColor);
    }

    public void OnEnterShopScreen() {
        Deselect();
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
                HideInfo();
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

    bool CanDragArtifact(Artifact artifact) {
        return true; //we might have artifacts later that are permanently glued to a cart.
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
            DoCartDrag();
        }else if (selectedArtifact != null) {
            DoArtifactDrag();
        }
        
        UpdateTrainCartPositionsSlowly();
    }

    private void DoArtifactDrag() {
        if (clickCart.action.WasPressedThisFrame()) {
            HideInfo();
            if (CanDragArtifact(selectedArtifact)) {
                BeginArtifactDrag();
            }
        }

        if (isDragStarted) {
            if (clickCart.action.IsPressed()) {
                CheckIfArtifactSnapping();
                if (!isSnapping) {
                    selectedArtifact.transform.position = GetMousePositionOnPlane() + offset;
                    selectedArtifact.transform.rotation = Quaternion.Slerp(selectedArtifact.transform.rotation, Quaternion.identity, slerpSpeed * Time.deltaTime);
                    offset = Vector3.Lerp(offset, Vector3.up/2f, lerpSpeed * Time.deltaTime);
                }
            } else {
                EndArtifactDrag();
            }
        }
    }

    public Cart sourceSnapCart;
    private void BeginArtifactDrag() {
        isSnapping = false;
        sourceSnapCart = selectedArtifact.GetComponentInParent<Cart>();
        selectedArtifact.DetachFromCart();
        selectedArtifact.GetComponent<Rigidbody>().isKinematic = true;
        selectedArtifact.GetComponent<Rigidbody>().useGravity = false;

        dragStartPos = GetMousePos();
        dragBasePos = selectedArtifact.transform.position;
        offset = dragBasePos - GetMousePositionOnPlane();
        isDragStarted = true;

        swapArtifact = null;
        swapCart = null;
        
        if (ArtifactsController.s.myArtifacts.Contains(selectedArtifact)) {
            ArtifactsController.s.ArtifactsChanged();
        } 

        /*if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.UpdateCartShopHighlights();
        } else {
            UpgradesController.s.UpdateCargoHighlights();
        }*/
    }


    public Artifact swapArtifact;
    public Cart swapCart;
    void CheckIfArtifactSnapping() {
        var carts = Train.s.carts;
        if (Mathf.Abs(GetMousePositionOnPlane().x) < 0.3f) {
            isSnapping = false;

            var zPos = GetMousePositionOnPlane().z;

            var totalLength = 0f;
            for (int i = 0; i < carts.Count; i++) {
                totalLength += carts[i].length;
            }

            var currentSpot = transform.localPosition - Vector3.back * (totalLength / 2f);

            for (int i = 0; i < carts.Count; i++) {
                var cart = carts[i];
                currentSpot += -Vector3.forward * cart.length;

                var distance = Mathf.Abs((currentSpot.z + (cart.length / 2f)) - zPos);
                if (currentSpot.z + (cart.length / 2f) < zPos && distance < cart.length * 4) {
                    if (cart.myAttachedArtifact != selectedArtifact) {
                        if (swapArtifact != null) {
                            swapArtifact.AttachToCart(swapCart);
                            swapCart = null;
                            swapArtifact = null;
                        }
                        
                        if (cart.myAttachedArtifact == null) {
                            selectedArtifact.AttachToCart(cart);

                        } else {
                            var canBeSwapped = CanDragArtifact(cart.myAttachedArtifact);

                            if (canBeSwapped) {
                                if (sourceSnapCart != null) {
                                    swapArtifact = cart.myAttachedArtifact;
                                    if (swapArtifact != null) {
                                        swapCart = cart;
                                        swapArtifact.AttachToCart(sourceSnapCart);
                                    }
                                } else {
                                    swapArtifact = cart.myAttachedArtifact;
                                    if (swapArtifact != null) {
                                        swapCart = cart;
                                        swapArtifact.DetachFromCart();
                                        swapArtifact.transform.position += Vector3.up/2f;
                                        //swapArtifact.transform.position = dragBasePos;
                                    }
                                }

                                selectedArtifact.AttachToCart(cart);
                            }
                        }
                    }

                    isSnapping = true;
                    return;
                }
            }
        }

        isSnapping = false;
        selectedArtifact.DetachFromCart();
        selectedArtifact.GetComponent<Rigidbody>().isKinematic = true;
        selectedArtifact.GetComponent<Rigidbody>().useGravity = false;

        
        if (swapArtifact != null) {
            swapArtifact.AttachToCart(swapCart);
            swapCart = null;
            swapArtifact = null;
        }
        
        /*if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.UpdateCartShopHighlights();
        } else {
            UpgradesController.s.UpdateCargoHighlights();
        }*/
    }

    
    
    private void EndArtifactDrag() {
        isDragStarted = false;
        if (!isSnapping) {
            selectedArtifact.GetComponent<Rigidbody>().isKinematic = false;
            selectedArtifact.GetComponent<Rigidbody>().useGravity = true;
        }
        

        /*if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.UpdateCartShopHighlights();
        } else {
            UpgradesController.s.UpdateCargoHighlights();
        }*/


        if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.SaveCartStateWithDelay();
            Train.s.SaveTrainState();
        }
    }

    private void DoCartDrag() {
        if (clickCart.action.WasPressedThisFrame()) {
            HideInfo();
            if (CanDragCart(selectedCart)) {
                BeginCartDrag();
            }
        }

        if (isDragStarted) {
            if (clickCart.action.IsPressed()) {
                CheckIfCartSnapping();
                if (!isSnapping) {
                    selectedCart.transform.position = GetMousePositionOnPlane() + offset;
                    selectedCart.transform.rotation = Quaternion.Slerp(selectedCart.transform.rotation, Quaternion.identity, slerpSpeed * Time.deltaTime);
                    offset = Vector3.Lerp(offset, Vector3.zero, lerpSpeed * Time.deltaTime);
                }
            } else {
                EndCartDrag();
            }
        }
    }

    private void EndCartDrag() {
        isDragStarted = false;
        if (!isSnapping) {
            selectedCart.GetComponent<Rigidbody>().isKinematic = false;
            selectedCart.GetComponent<Rigidbody>().useGravity = true;
        }

        if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.SnapDestinationCargos(selectedCart);
        }

        if (selectedCart.isMysteriousCart &&
            !(selectedCart.myLocation == UpgradesController.CartLocation.train || selectedCart.myLocation == UpgradesController.CartLocation.cargoDelivery)) {
            UpgradesController.s.RemoveCartFromShop(selectedCart);
            Train.s.AddCartAtIndex(1, selectedCart);
            selectedCart.GetComponent<Rigidbody>().isKinematic = true;
            selectedCart.GetComponent<Rigidbody>().useGravity = false;
        }

        if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.UpdateCartShopHighlights();
        } else {
            UpgradesController.s.UpdateCargoHighlights();
        }


        if (PlayStateMaster.s.isShop()) {
            UpgradesController.s.SaveCartStateWithDelay();
            Train.s.SaveTrainState();
        }
    }

    private void BeginCartDrag() {
        currentSnapLoc = null;
        isSnapping = false;
        sourceSnapLocation = selectedCart.GetComponentInParent<SnapCartLocation>();
        prevCartTrainSnapIndex = -1;
        selectedCart.transform.SetParent(null);
        selectedCart.GetComponent<Rigidbody>().isKinematic = true;
        selectedCart.GetComponent<Rigidbody>().useGravity = false;

        dragStartPos = GetMousePos();
        dragBasePos = selectedCart.transform.position;
        offset = dragBasePos - GetMousePositionOnPlane();
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

    public SnapCartLocation currentSnapLoc;
    void CheckIfCartSnapping() {
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
                    var snapLocationCanAcceptCart = (!snapLocation.onlySnapCargo || selectedCart.isCargo) && (!snapLocation.onlySnapMysteriousCargo || selectedCart.isMysteriousCart);
                    var snapLocationEmpty = snapLocation.snapTransform.childCount == 0;
                    var canSnap = snapLocationValidAndNew && snapLocationCanAcceptCart && snapLocationEmpty && !snapLocation.snapNothing;

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
                                canBeSwapped = !swapCart.isMysteriousCart && !swapCart.isCargo && !swapCart.isMainEngine;
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
            
            if (cart.artifactParent.childCount > 0) {
                var artifact = cart.artifactParent.GetChild(0);
                artifact.transform.localPosition = Vector3.Lerp(artifact.transform.localPosition, Vector3.zero, 5*lerpSpeed * Time.deltaTime);
                artifact.transform.localRotation = Quaternion.Slerp(artifact.transform.localRotation, Quaternion.identity, 5*slerpSpeed * Time.deltaTime);
            }

            if (cart == selectedCart && !cart.isMainEngine)
                currentSpot -= Vector3.up * (isDragStarted ? 0.4f : 0.05f);


            currentSpot += -Vector3.forward * cart.length;
            var index = i;
            cart.name = $"Cart {index}";
        }
    }

    public float repairAmountPerClick = 50f; // dont use this
    public float repairAmountMultiplier = 1; 
    public float reloadAmountPerClick = 2; // dont use this
    public float reloadAmountMultiplier = 1;
    public float shieldUpAmountPerClick = 100f; // dont use this
    public float shieldUpAmountMultiplier = 1; 

    public float GetReloadAmount() {
        return reloadAmountPerClick * reloadAmountMultiplier;
    }

    public float GetRepairAmount() {
        return repairAmountPerClick * repairAmountMultiplier;
    }
    
    public float GetShieldUpAmount() {
        return shieldUpAmountPerClick * shieldUpAmountMultiplier;
    }

    

    void CheckAndDoClick() {
        if (selectedCart != null) {
            if (clickCart.action.WasPerformedThisFrame()) {
                HideInfo();
                switch (currentSelectMode) {
                    case SelectMode.cart:
                        switch (currentCursorState) {
                            case CursorState.empty:
                                // do nothing
                                break;
                            case CursorState.repair:
                                selectedCart.GetHealthModule().Repair(GetRepairAmount());
                                break;
                            case CursorState.shieldUp:
                                selectedCart.GetHealthModule().ShieldUp(GetShieldUpAmount());
                                break;
                            case CursorState.reload_basic:
                                selectedCart.GetComponentInChildren<ModuleAmmo>()?.Reload(GetReloadAmount());
                                break;
                            case CursorState.reload_fire:
                            case CursorState.reload_sticky:
                                selectedCart.GetComponentInChildren<ModuleAmmo>()?.ApplyBulletEffect(currentCursorState);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case SelectMode.topButton:
                        var stateChanger = selectedCart.GetComponentInChildren<CursorStateChanger>();
                        var directControllable = selectedCart.GetComponentInChildren<DirectControllable>();

                        if (directControllable) {
                            DirectControlMaster.s.AssumeDirectControl(selectedCart.GetComponentInChildren<DirectControllable>());
                        }else if (stateChanger) {
                            SetCursorState(stateChanger.targetState, stateChanger.color);
                        }
                        break;
                    case SelectMode.engineBoost:
                        SpeedController.s.ActivateBoost();
                        break;
                }
            }
        }else if (selectedEnemy != null) {
            if (clickCart.action.WasPerformedThisFrame()) {
                HideInfo();
            }
        }
    }

    public void UIRepair(Cart cart) {
        switch (currentCursorState) {
            case CursorState.repair:
                cart.GetHealthModule()?.Repair(GetRepairAmount());
                break;
            case CursorState.shieldUp:
                cart.GetHealthModule()?.ShieldUp(GetShieldUpAmount());
                break;
        }
        
        var moduleAmmo = cart.GetComponentInChildren<ModuleAmmo>();
        if (moduleAmmo != null) {
            switch (currentCursorState) {
                case CursorState.reload_basic:
                    moduleAmmo.Reload(GetReloadAmount());
                    break;
                case CursorState.reload_fire:
                case CursorState.reload_sticky:
                    moduleAmmo.ApplyBulletEffect(currentCursorState);
                    break;
            }
        }
    }

    public void CartHPUIButton(Cart cart) {
        if (PlayStateMaster.s.isCombatInProgress()) {
            var moduleAmmo = cart.GetComponentInChildren<ModuleAmmo>();
            if (moduleAmmo != null) {
                switch (currentCursorState) {
                    case CursorState.reload_basic:
                        moduleAmmo.Reload(GetReloadAmount());
                        break;
                    case CursorState.reload_fire:
                    case CursorState.reload_sticky:
                        moduleAmmo.ApplyBulletEffect(currentCursorState);
                        break;
                }
            }

            if (cart.GetComponentInChildren<DirectControllable>()) {
                DirectControlMaster.s.AssumeDirectControl(cart.GetComponentInChildren<DirectControllable>());
            }

            if (cart.GetComponentInChildren<EngineBoostable>()) {
                SpeedController.s.ActivateBoost();
            }

            var state = cart.GetComponentInChildren<CursorStateChanger>();
            if (state != null) {
                SetCursorState(state.targetState, state.GetColor());
            }
        }
    }


    public CursorState currentCursorState = CursorState.repair;
    public GameObject cursorStateObject;
    public Sprite cursorState_Repair;
    public Sprite cursorState_ShieldUp;
    public Sprite cursorState_Reload_Basic;
    public Sprite cursorState_Reload_Fire;
    public Sprite cursorState_Reload_Sticky;
    
    void SetCursorState(CursorState targetState, Color stateColor) {
        if (currentCursorState != targetState) {
            var targetSprite = cursorState_Repair;
            cursorStateObject.gameObject.SetActive(true);
            switch (targetState) {
                case CursorState.empty:
                    cursorStateObject.gameObject.SetActive(false);
                    break;
                case CursorState.repair:
                    targetSprite = cursorState_Repair;
                    break;
                case CursorState.shieldUp:
                    targetSprite = cursorState_ShieldUp;
                    break;
                case CursorState.reload_basic:
                    targetSprite = cursorState_Reload_Basic;
                    break;
                case CursorState.reload_fire:
                    targetSprite = cursorState_Reload_Fire;
                    break;
                case CursorState.reload_sticky:
                    targetSprite = cursorState_Reload_Sticky;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetState), targetState, null);
            }

            activeCursorStateColor = stateColor;
            cursorStateObject.GetComponentInChildren<Image>().sprite = targetSprite;
            currentCursorState = targetState;
            
            GamepadControlsHelper.s.SetClickActionIcon(targetSprite);
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
        cart, topButton, engineBoost, emptyCart
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
    [ReadOnly]
    public Color selectingTopButtonColor;

    [ReadOnly]
    public Color activeCursorStateColor;
    void CastRayToOutlineCart() {
        RaycastHit hit;
        Ray ray = GetRay();

        if (Physics.SphereCast(ray, GetSphereCastRadius(), out hit, 100f, LevelReferences.s.buildingLayer)) {
            var lastSelectMode = currentSelectMode;
            currentSelectMode = SelectMode.emptyCart;
            
            if(hit.collider.GetComponentInParent<ModuleHealth>() && canRepair)
                currentSelectMode = SelectMode.cart;
            
            var topButton = hit.collider.GetComponentInParent<IShowButtonOnCartUIDisplay>();
            if (SettingsController.GamepadMode() && topButton == null) {
                topButton = hit.rigidbody.gameObject.GetComponentInChildren<IShowButtonOnCartUIDisplay>();
            }
            if (topButton != null) {
                currentSelectMode = SelectMode.topButton;
                selectingTopButtonColor = topButton.GetColor();
            }
            
            /*var reloadable = hit.collider.GetComponentInParent<Reloadable>();
            if (SettingsController.GamepadMode() && reloadable == null) {
                reloadable = hit.rigidbody.gameObject.GetComponentInChildren<Reloadable>();
            }
            if (reloadable != null && canReload) {
                currentSelectMode = SelectMode.reload;
            }*/
            
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
                SelectBuilding(cart, true);

            } else {
                if (PlayStateMaster.s.isShopOrEndGame() && showDetailClick.action.WasPerformedThisFrame() /*|| (holdOverTimer > infoShowTime && !SettingsController.GamepadMode())*/) {
                    ShowSelectedThingInfo();
                }
            }

        } else {
            if(selectedCart != null)
                Deselect();
        }
    }
    
    void CastRayToOutlineEnemy() {
        RaycastHit hit;
        Ray ray = GetRay();

        if (Physics.SphereCast(ray, GetSphereCastRadius(), out hit, 100f, LevelReferences.s.enemyLayer)) {
            var enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != selectedEnemy) {
                SelectEnemy(enemy, true);
            } else {
                if (showDetailClick.action.WasPerformedThisFrame() /*|| (holdOverTimer > infoShowTime && !SettingsController.GamepadMode())*/) {
                    ShowSelectedThingInfo();
                }
            }

        } else {
            if(selectedEnemy != null)
                Deselect();
        }
    }
    
    
    void CastRayToOutlineArtifact() {
        RaycastHit hit;
        Ray ray = GetRay();

        if (Physics.SphereCast(ray, GetSphereCastRadius(), out hit, 100f, LevelReferences.s.artifactLayer)) {
            var artifact = hit.collider.GetComponentInParent<Artifact>();
            if (artifact != selectedArtifact) {
                SelectArtifact(artifact, true);
            } else {
                if (showDetailClick.action.WasPerformedThisFrame() /*|| (holdOverTimer > infoShowTime && !SettingsController.GamepadMode())*/) {
                    ShowSelectedThingInfo();
                }
            }
        } else {
            if(selectedArtifact != null)
                Deselect();
        }
    }


    private bool infoCardActive = false;
    void ShowSelectedThingInfo() {
        if (!infoCardActive) {
            infoCardActive = true;
            if(selectedCart != null)
                infoCard.SetUp(selectedCart);
            else if (selectedEnemy != null)
                infoCard.SetUp(selectedEnemy);
            else if (selectedArtifact != null)
                infoCard.SetUp(selectedArtifact);
            else
                infoCardActive = false;
        } 
    }

    void HideInfo() {
        infoCardActive = false;
        infoCard.Hide();
    }

    public void Deselect() {
        if (selectedCart != null) {
            var cart = selectedCart;
            selectedCart = null;
            SelectBuilding(cart, false);
        }

        if (selectedEnemy != null) {
            var enemy = selectedEnemy;
            selectedEnemy = null;
            SelectEnemy(enemy, false);
        }
        
        if (selectedArtifact != null) {
            var artifact = selectedArtifact;
            selectedArtifact = null;
            SelectArtifact(artifact, false);
        }
        
        HideInfo();
    }

    public void SelectEnemy(EnemyHealth enemy, bool isSelecting, bool showShowDetails = true) {
        if(isSelecting && enemy == selectedEnemy)
            return;
        //Debug.Log("selecting enemy");
        Deselect();

        Outline outline = null;
        if(enemy != null)
            outline = enemy.GetComponentInChildren<Outline>();
        
        if (isSelecting) {
            if(showShowDetails)
                GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.showDetails);
            selectedEnemy = enemy;
        } else {
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.showDetails);
        }

        if (enemy != null) {
            outline.enabled = isSelecting;
        }

        OnSelectEnemy?.Invoke(enemy, isSelecting);
    }
    
    void SelectArtifact(Artifact artifact, bool isSelecting) {
        Deselect();

        Outline outline = null;
        if(artifact != null)
            outline = artifact.GetComponentInChildren<Outline>();
        
        if (isSelecting) {
            Color myColor = moveColor;
            if (PlayStateMaster.s.isShopOrEndGame()) {
                GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.showDetails);
                GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.moveArtifact);
                if (!CanDragArtifact(artifact)) {
                    myColor = cantActColor;
                    GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.moveArtifact);
                }
            } else {
                myColor = cantActColor;
                GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.showDetails);
            }
            selectedArtifact = artifact;
            
            outline.OutlineColor = myColor;
        } else {
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.moveArtifact);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.showDetails);
        }

        if (outline != null) {
            outline.enabled = isSelecting;
        }

        OnSelectArtifact?.Invoke(artifact, isSelecting);
    }


    [HideInInspector]
    public UnityEvent<Cart, bool> OnSelectBuilding = new UnityEvent<Cart, bool>();
    public UnityEvent<EnemyHealth, bool> OnSelectEnemy = new UnityEvent<EnemyHealth, bool>();
    public UnityEvent<Artifact, bool> OnSelectArtifact = new UnityEvent<Artifact, bool>();
    public UnityEvent<GateScript, bool> OnSelectGate = new UnityEvent<GateScript, bool>();
    void SelectBuilding(Cart building, bool isSelecting) {
        Deselect();
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
                switch (currentSelectMode) {
                    case SelectMode.cart:
                        if (CanCurrentStateAct(building)) {
                            myColor = activeCursorStateColor;
                            GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.click);
                        } else {
                            myColor = cantActColor;
                        }

                        break;
                    case SelectMode.topButton:
                        myColor = selectingTopButtonColor;
                        GamepadControlsHelper.s.AddPossibleActions(GamepadControlsHelper.PossibleActions.selectTopButton);
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
            selectedCart = building;
        } else {
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.move);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.repair);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.reload);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.directControl);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.showDetails);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.engineBoost);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.selectTopButton);
            GamepadControlsHelper.s.RemovePossibleAction(GamepadControlsHelper.PossibleActions.click);
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


    public bool CanCurrentStateAct(Cart cart) {
        switch (currentCursorState) {
            case CursorState.empty:
                return false;
            case CursorState.repair:
                return true;
            case CursorState.shieldUp:
                return cart.GetHealthModule().maxShields > 0;
            case CursorState.reload_basic:
            case CursorState.reload_fire:
            case CursorState.reload_sticky:
                return cart.GetComponentInChildren<ModuleAmmo>() != null;
            default:
                throw new ArgumentOutOfRangeException();
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


