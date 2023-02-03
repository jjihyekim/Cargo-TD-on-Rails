using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionsController : MonoBehaviour {
    public static PlayerActionsController s;

    private void Awake() {
        s = this;
    }
    
    public InputActionReference click;
    public InputActionReference cancel;
    
    protected void OnEnable() {
        click.action.Enable();
        cancel.action.Enable();
        click.action.performed += ActivateActionOnActiveBuilding;
        cancel.action.performed += Cancel;
    }

    protected void OnDisable() {
        click.action.Disable();
        cancel.action.Disable();
        click.action.performed -= ActivateActionOnActiveBuilding;
        cancel.action.performed -= Cancel;
    }

    void Cancel(InputAction.CallbackContext callbackContext) {
        LeaveAllModes();
    }
    
    void OnLevelIsOver (){
        LeaveAllModes();
        
        stopRepairingList.Clear();
        for (int i = 0; i < currentlyRepairing.Count; i++) {
            stopRepairingList.Add(currentlyRepairing[i]);
        }
        for (int i = 0; i < stopRepairingList.Count; i++) {
            StopRepairing(stopRepairingList[i]);
        }
        stopRepairingList.Clear();
    }

    public MiniGUI_ActionButton directActionButton;
    public MiniGUI_ActionButton reloadActionButton;
    public MiniGUI_ActionButton repairActionButton;
    public MiniGUI_ActionButton shopRepairActionButton;
    public MiniGUI_ActionButton shopReloadActionButton;
    public MiniGUI_ActionButton shopMoveActionButton;
    public MiniGUI_ActionButton shopSellActionButton;

    private void Start() {
        StarterUIController.s.OnLevelStarted.AddListener(OnLevelStart);
        DeselectObject();
        PlayerBuildingController.s.startBuildingEvent.AddListener(LeaveAllModes);
        currentlyRepairingUIThing.gameObject.SetActive(false);
        StarterUIController.s.OnLevelStarted.AddListener(LeaveAllModes);
        MissionWinFinisher.s.OnLevelFinished.AddListener(OnLevelIsOver);
        
        repairActionButton.OnToggleValueChanged.AddListener(HealthBarToggle);
        shopRepairActionButton.OnToggleValueChanged.AddListener(HealthBarToggle);
        
        reloadActionButton.OnToggleValueChanged.AddListener(AmmoBarToggle);
        shopReloadActionButton.OnToggleValueChanged.AddListener(AmmoBarToggle);
        
        repairActionButton.OnButtonPressed.AddListener(EnterRepairMode);
        reloadActionButton.OnButtonPressed.AddListener(EnterReloadMode);
        directActionButton.OnButtonPressed.AddListener(EnterDirectControlMode);
        
        
        shopRepairActionButton.OnButtonPressed.AddListener(EnterShopRepairMode);
        shopReloadActionButton.OnButtonPressed.AddListener(EnterReloadMode);
        shopMoveActionButton.OnButtonPressed.AddListener(EnterShopMoveMode);
        shopSellActionButton.OnButtonPressed.AddListener(EnterShopSellMode);
    }

    void OnLevelStart()
    {
        if (PlayerPrefs.GetInt("ShowHealthBars", 1) == 0) {
            repairActionButton.OverrideSliderValue(false);
            HealthBarToggle(false);
        } else {
            HealthBarToggle(true);
        }

        if (PlayerPrefs.GetInt("ShowAmmoBars", 1) == 0) {
            reloadActionButton.OverrideSliderValue(false);
            AmmoBarToggle(false);
        } else {
            AmmoBarToggle(true);
        }

        LeaveAllModes();
    }


    void HealthBarToggle(bool value) {
        MiniGUI_HealthBar.showHealthBars = value;
        PlayerPrefs.SetInt("ShowHealthBars", value ? 1 : 0);
        repairActionButton.OverrideSliderValue(value);
        shopRepairActionButton.OverrideSliderValue(value);
    }

    void AmmoBarToggle(bool value) {
        MiniGUI_HealthBar.showAmmoBars = value;
        PlayerPrefs.SetInt("ShowAmmoBars", value ? 1 : 0);
        reloadActionButton.OverrideSliderValue(value);
        shopReloadActionButton.OverrideSliderValue(value);
    }

    void HealthBarShow() {
        MiniGUI_HealthBar.showHealthBars = true;
    }

    void HealthBarReset() {
        MiniGUI_HealthBar.showHealthBars = PlayerPrefs.GetInt("ShowHealthBars", 1) == 1;
    }
    
    void AmmoBarShow() {
        MiniGUI_HealthBar.showAmmoBars = true;
    }

    void AmmoBarReset() {
        MiniGUI_HealthBar.showAmmoBars = PlayerPrefs.GetInt("ShowAmmoBars", 1) == 1;
    }
    
    public enum  ActionModes {
        none, repair, reload, directControl, shopRepair, shopMove, shopSell
    }

    public ActionModes currentMode = ActionModes.none;


    [ColorUsage(true, true)] public Color nonSelectedColor =Color.black;
    [ColorUsage(true, true)] public Color repairCurrentlyRepairingColor = Color.green;
    [ColorUsage(true, true)] public Color repairFullHPColor = Color.green;
    [ColorUsage(true, true)] public Color repairNoHpColor = Color.blue;
    [ColorUsage(true, true)] public Color reloadColor = Color.blue;
    [ColorUsage(true, true)] public Color directControlColor = Color.blue;
    [ColorUsage(true, true)] public Color moveColor = Color.blue;
    [ColorUsage(true, true)] public Color sellColor = Color.blue;

    void ResetMaterials() {
        var trainBuildings = Train.s.GetComponentsInChildren<TrainBuilding>();

        for (int i = 0; i < trainBuildings.Length; i++) {
            SetBuildingColor(trainBuildings[i], nonSelectedColor);
        }   
    }

    void SetBuildingColor(TrainBuilding building, Color color) {
        var renderers = building.GetComponentsInChildren<MeshRenderer>();
        for (int j = 0; j < renderers.Length; j++) {
            var rend = renderers[j];
            rend.material.SetColor("OverlayColor", color);
        }
    }
    
    void EnterRepairMode() {
        if(reEnterTimer > 0 && lastMode == ActionModes.repair)
            return;
        if (currentMode != ActionModes.repair) {
            var trainBuildings = Train.s.GetComponentsInChildren<TrainBuilding>();

            for (int i = 0; i < trainBuildings.Length; i++) {
                if (IsRepairable(trainBuildings[i])) {
                    SetBuildingColor(trainBuildings[i], GetRepairColor(trainBuildings[i]));
                }
            }

            currentMode = ActionModes.repair;
            HealthBarShow();
            EnterAMode();
        } else {
            LeaveAllModes();
        }
    }
    
    void EnterShopRepairMode() {
        if(reEnterTimer > 0 && lastMode == ActionModes.shopRepair)
            return;
        if (currentMode != ActionModes.shopRepair) {
            var trainBuildings = Train.s.GetComponentsInChildren<TrainBuilding>();

            for (int i = 0; i < trainBuildings.Length; i++) {
                if (IsRepairable(trainBuildings[i])) {
                    SetBuildingColor(trainBuildings[i], GetRepairColor(trainBuildings[i]));
                }
            }

            currentMode = ActionModes.shopRepair;
            HealthBarShow();
            EnterAMode();
        } else {
            LeaveAllModes();
        }
    }

    Color GetRepairColor(TrainBuilding building) {
        if (currentlyRepairing.Contains(building)) {
            return repairCurrentlyRepairingColor;
        } else {
            var healthModule = building.GetComponent<ModuleHealth>();
            var hpPercent = healthModule.currentHealth / healthModule.maxHealth;

            return Color.Lerp(repairNoHpColor, repairFullHPColor, hpPercent);
        }
    }

    bool IsRepairable(TrainBuilding building) {
        return building.GetComponent<RepairAction>();
    }


    void EnterReloadMode() {
        if(reEnterTimer > 0 && lastMode == ActionModes.reload)
            return;
        if (currentMode != ActionModes.reload) {
            var trainBuildings = Train.s.GetComponentsInChildren<TrainBuilding>();

            for (int i = 0; i < trainBuildings.Length; i++) {
                if (IsReloadable(trainBuildings[i])) {
                    SetBuildingColor(trainBuildings[i], reloadColor);
                }
            }

            currentMode = ActionModes.reload;
            AmmoBarShow();
            EnterAMode();
        }else {
            LeaveAllModes();
        }
    }

    bool IsReloadable(TrainBuilding building) {
        return building.GetComponent<ReloadAction>();
    }

    void EnterDirectControlMode() {
        if(reEnterTimer > 0 && lastMode == ActionModes.directControl)
            return;
        if (currentMode != ActionModes.directControl) {
            var trainBuildings = Train.s.GetComponentsInChildren<TrainBuilding>();

            for (int i = 0; i < trainBuildings.Length; i++) {
                if (IsDirectControllable(trainBuildings[i])) {
                    SetBuildingColor(trainBuildings[i], directControlColor);
                }
            }

            currentMode = ActionModes.directControl;
            EnterAMode();
        } else {
            LeaveAllModes();
        }
    }

    bool IsDirectControllable(TrainBuilding building) {
        return building.GetComponent<DirectControlAction>();
    }
    
    void EnterShopMoveMode() {
        if(reEnterTimer > 0 && lastMode == ActionModes.shopMove)
            return;
        if (currentMode != ActionModes.shopMove) {
            var trainBuildings = Train.s.GetComponentsInChildren<TrainBuilding>();

            for (int i = 0; i < trainBuildings.Length; i++) {
                if (IsMoveable(trainBuildings[i])) {
                    SetBuildingColor(trainBuildings[i], moveColor);
                }
            }

            currentMode = ActionModes.shopMove;
            EnterAMode();
        } else {
            LeaveAllModes();
        }
    }
    
    bool IsMoveable(TrainBuilding building) {
        return building.GetComponent<MoveModuleAction>();
    }
    
    void EnterShopSellMode() {
        if(reEnterTimer > 0 && lastMode == ActionModes.shopSell)
            return;
        if (currentMode != ActionModes.shopSell) {
            var trainBuildings = Train.s.GetComponentsInChildren<TrainBuilding>();

            for (int i = 0; i < trainBuildings.Length; i++) {
                if (IsSellable(trainBuildings[i])) {
                    SetBuildingColor(trainBuildings[i], sellColor);
                }
            }

            currentMode = ActionModes.shopSell;
            EnterAMode();
        } else {
            LeaveAllModes();
        }
    }

    bool IsSellable(TrainBuilding building) {
        return building.GetComponent<SellAction>();
    }

    void EnterAMode() {
        PlayerModuleSelector.s.canSelectModules = false;
    }

    public float reEnterTimer = 0;
    public ActionModes lastMode;
    void LeaveAllModes() {
        PlayerModuleSelector.s.canSelectModules = true;
        ResetMaterials();
        currentMode = ActionModes.none;
        costInfo.Deactivate();
        DeselectObject();
        
        AmmoBarReset();
        HealthBarReset();
        reEnterTimer = 0.2f;
    }

    public MiniGUI_PlayerActionInfo costInfo;

    public List<TrainBuilding> currentlyRepairing = new List<TrainBuilding>();
    public UIElementFollowWorldTarget currentlyRepairingUIThing;
    void ActivateActionOnActiveBuilding(InputAction.CallbackContext callbackContext) {
        if (currentMode != ActionModes.none) {
            if (activeBuilding != null) {
                switch (currentMode) {
                    case ActionModes.reload:
                        if (IsReloadable(activeBuilding)) {
                            activeBuilding.GetComponent<ReloadAction>().EngageAction();
                        }
                        break;
                    case ActionModes.repair:
                        if (IsRepairable(activeBuilding)) {
                            if (!currentlyRepairing.Contains(activeBuilding)) { 
                                StartRepairing(activeBuilding);
                            } else { 
                                StopRepairing(activeBuilding);
                            }
                        }
                        break;
                    case ActionModes.directControl:
                        if (IsDirectControllable(activeBuilding)) {
                            var directControlTarget = activeBuilding.GetComponent<DirectControlAction>();
                            LeaveAllModes();
                            DirectControlMaster.s.AssumeDirectControl(directControlTarget);
                        }
                        break;
                    case ActionModes.shopRepair:
                        if (IsRepairable(activeBuilding)) {
                            activeBuilding.GetComponent<RepairAction>().EngageAction();
                        }
                        break;
                    case ActionModes.shopMove:
                        if (IsMoveable(activeBuilding)) {
                            ActivateMoveOnModule(activeBuilding);
                        }
                        break;
                    case ActionModes.shopSell:
                        if (IsSellable(activeBuilding)) {
                            activeBuilding.GetComponent<SellAction>().EngageAction();
                        }
                        break;
                }
            } else {
                LeaveAllModes();
            }
        }
    }

    void StartRepairing(TrainBuilding building) {
        currentlyRepairing.Add(building);
        var newRepairUIThing = Instantiate(currentlyRepairingUIThing.gameObject);
        newRepairUIThing.GetComponent<UIElementFollowWorldTarget>().SetUp(building.GetUITargetTransform(false));
        newRepairUIThing.SetActive(true);
        building.currentlyRepairingUIThing = newRepairUIThing;
    }
    void StopRepairing(TrainBuilding building) {
        currentlyRepairing.Remove(building);
        if (building.currentlyRepairingUIThing != null) {
            Destroy(building.currentlyRepairingUIThing);
            building.currentlyRepairingUIThing = null;
        }
    }

    private TrainBuilding movingBuilding;
    void ActivateMoveOnModule(TrainBuilding building) {
        movingBuilding = building;
        LeaveAllModes();
        movingBuilding.mySlot.TemporaryRemoval(movingBuilding);
        PlayerBuildingController.s.StartBuilding(DataHolder.s.GetBuilding(movingBuilding.uniqueName), BuildingDoneCallback, GetTheFinishedBuilding, true,false);
        movingBuilding.SetHighlightState(true);
    }
    
    bool BuildingDoneCallback(bool isSuccess) {
        if (isSuccess) {
            Destroy(movingBuilding.gameObject);
            Train.s.SaveTrainState();
        } else {
            movingBuilding.SetHighlightState(false);
            movingBuilding.mySlot.TemporaryRemoveReversal();
        }

        Invoke(nameof(EnterShopMoveMode), 0.1f);

        return false;
    }

    void GetTheFinishedBuilding(TrainBuilding newBuilding) {
        newBuilding.SetCurrentHealth(movingBuilding.GetComponent<ModuleHealth>().currentHealth);

        var ammo = movingBuilding.GetComponent<ModuleAmmo>();

        if (ammo != null) {
            var newAmmo = newBuilding.GetComponent<ModuleAmmo>();

            newAmmo.SetAmmo(ammo.curAmmo);
        }
    }

    private float repairTimer;
    public float repairDelay = 2;
    public int repairAmount = 20;
    private List<TrainBuilding> stopRepairingList = new List<TrainBuilding>();
    void Update() {
        if (reEnterTimer > 0) {
            reEnterTimer -= Time.deltaTime;
        } else {
            lastMode = currentMode;
        }

        if (currentMode != ActionModes.none) {
            CastRayToSelectBuilding();
        }

        if (currentlyRepairing.Count > 0) {
            if (repairTimer <= 0) {
                for (int i = 0; i < currentlyRepairing.Count; i++) {
                    var repairModule = currentlyRepairing[i].GetComponent<RepairAction>();
                    var healthModule = currentlyRepairing[i].GetComponent<ModuleHealth>();
                    var missingHealth = healthModule.maxHealth - healthModule.currentHealth;
                    var currentRepairAmount = Mathf.Min(missingHealth, repairAmount);

                    if (currentRepairAmount > 0) {
                        var cost = repairModule.GetCostPerHealth(currentRepairAmount);

                        if (MoneyController.s.HasResource(ResourceTypes.scraps, cost)) {
                            MoneyController.s.ModifyResource(ResourceTypes.scraps, -cost);

                            repairModule.Repair(repairAmount);
                        }
                    } else {
                        stopRepairingList.Add(currentlyRepairing[i]);
                    }
                }

                for (int i = 0; i < stopRepairingList.Count; i++) {
                    StopRepairing(stopRepairingList[i]);
                }
                stopRepairingList.Clear();
                
                repairTimer = repairDelay;
            } 
        }
        repairTimer -= Time.deltaTime;
    }


    private Slot activeSlot;
    private int activeIndex;
    private int lastRaycastIndex;
    private TrainBuilding activeBuilding;


    void CastRayToSelectBuilding() {
        RaycastHit hit;
        Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, 100f, LevelReferences.s.buildingLayer)) {
            var slot = hit.collider.gameObject.GetComponentInParent<Slot>();

            if (slot != null) {
                var index = PlayerBuildingController.NormalToIndex(hit.normal);
                if (slot != activeSlot) {
                    // we clicked on a new slot
                    //DeselectObject();

                    if (index != -1) {
                        SelectObject(slot, index);
                    }

                } else {
                    // if it is still the same slot but a different side, then pick the new slot
                    if (index != -1 && index != lastRaycastIndex) {
                        SelectObject(slot, index);
                    } /*else {
                        HideModuleActionView();
                    }*/
                }

                lastRaycastIndex = index;
            } else {
                DeselectObject();
            }
        } else {
            DeselectObject();
        }
    }
    
    public void DeselectObject() {
        if (activeSlot != null) {
            if (activeBuilding != null)
                SelectBuilding(activeBuilding, false);

            activeSlot = null;
            activeIndex = -2;
            lastRaycastIndex = -2;
            activeBuilding = null;
        }
        
        costInfo.Deactivate();
    }

    void SelectObject(Slot slot, int index) {
        if (slot != activeSlot || index != activeIndex) {
            activeSlot = slot;
            activeIndex = index;

            if (activeBuilding != null)
                SelectBuilding(activeBuilding, false);

            activeBuilding = slot.myBuildings[index];
            if (activeBuilding == null) {
                // if there is nothing in this slot then try other slots until we find one
                /*for (int i = 0; i < slot.myBuildings.Length; i++) {
                    if (slot.myBuildings[i] != null) {
                        activeBuilding = slot.myBuildings[i];
                        activeIndex = i;
                    }
                }*/
                
                // try forward/backward direction
                if (index == 0)
                    index = 1;
                else if (index == 1)
                    index = 0;
                
                activeBuilding = slot.myBuildings[index];
            }

            if (activeBuilding != null) {
                SelectBuilding(activeBuilding, true);
            } 
        }
    }


    int GetRepairCostPerSecond(TrainBuilding building) {
        var repairModule = building.GetComponent<RepairAction>();
        var cost = repairModule.GetCostPerHealth(repairAmount);

        return Mathf.RoundToInt(cost);
    }

    int GetTotalRepairCost(TrainBuilding building) {
        var repairModule = building.GetComponent<RepairAction>();
        var cost = repairModule.cost;

        return Mathf.RoundToInt(cost);
    }
    
    public void SelectBuilding(TrainBuilding building, bool isSelected) {
        if (building != null) {
            if (isSelected) {
                switch (currentMode) {
                    case ActionModes.reload:
                        if (!IsReloadable(building)) {
                            return;
                        } else {
                            var reloadAction = building.GetComponent<ReloadAction>();
                            costInfo.ShowReloadCost(reloadAction.cost, reloadAction.myType);
                        }
                        break;
                    case ActionModes.repair:
                        if (!IsRepairable(building)) {
                            return;
                        } else {
                            var repairCostPerSecond = GetRepairCostPerSecond(building);
                            costInfo.ShowRepairInfo(repairCostPerSecond, ResourceTypes.scraps);
                        }
                        break;
                    case ActionModes.directControl:
                        if (!IsDirectControllable(building)) {
                            return;
                        } else {
                            costInfo.ShowDirectControlInfo();
                        }
                        break;
                    case ActionModes.shopRepair:
                        if (!IsRepairable(building)) {
                            return;
                        } else {
                            var totalRepairCost = GetTotalRepairCost(building);
                            costInfo.ShowShopRepairInfo(totalRepairCost, ResourceTypes.scraps);
                        }
                        break;
                    case ActionModes.shopMove:
                        if (!IsMoveable(building)) {
                            return;
                        } else {
                            costInfo.ShowMoveInfo();
                        }
                        break;
                    case ActionModes.shopSell:
                        if (!IsSellable(building)) {
                            return;
                        } else {
                            var refund = building.GetComponent<SellAction>().cost;
                            costInfo.ShowShopSellInfo(refund);
                        }
                        break;
                }
            }


            building.SetHighlightState(isSelected);
        }
    }
}
