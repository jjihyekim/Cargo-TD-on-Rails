using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerModuleSelector : MonoBehaviour {
    public static PlayerModuleSelector s;

    private void Awake() {
        s = this;
    }

    public InputActionReference click;
    public InputActionReference scroll;

    public MiniGUI_ModuleActionSelection activeActionSelection;


    private void Start() {
        DeselectObject();
    }


    protected void OnEnable() {
        click.action.Enable();
        scroll.action.Enable();
        click.action.performed += ActivateActionDisplayOnActiveObject;
    }



    protected void OnDisable() {
        click.action.Disable();
        scroll.action.Disable();
        click.action.performed -= ActivateActionDisplayOnActiveObject;
        if(activeActionSelection != null)
            HideModuleActionSelector();
        DeselectObject();
    }

    
    private void ActivateActionDisplayOnActiveObject(InputAction.CallbackContext obj) {
        if (EventSystem.current.IsPointerOverGameObject()) {
            // dont act if we are hovering over ui
            return;
        }
        
        if (!activeActionSelection.IsMouseOverMenu()) {
            // we dont want to hide menu in case the player clicks on it
            if (activeBuilding != null) {
                activeActionSelection.gameObject.SetActive(true);
                activeActionSelection.SetUp(activeBuilding);
            } else {
                HideModuleActionSelector();
            }
        }
    }

    public void HideModuleActionSelector() {
        activeActionSelection.gameObject.SetActive(false);
    }


    private void Update() {
        CastRayToSelectBuilding();

        if (isMouseOverSelectedObject) {
            CameraScroll.s.canZoom = false;
            ProcessScroll(scroll.action.ReadValue<float>());
        } else {
            CameraScroll.s.canZoom = true;
        }
    }
    


    private Slot activeSlot;
    private int activeIndex;
    private int lastRaycastIndex;
    private TrainBuilding activeBuilding;

    public bool playerBuildingDisableOverride = false;
    public bool playerBuildingSkipOneTimeOverride = false;

    public bool isMouseOverSelectedObject = false;
    void CastRayToSelectBuilding() {
        if (playerBuildingDisableOverride) {
            DeselectObject();
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject()) {
            // dont act if we are hovering over ui
            return;
        }


        RaycastHit hit;
        Ray ray = LevelReferences.s.mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hit, 100f, PlayerBuildingController.s.buildingLayerMask)) {
            var slot = hit.collider.gameObject.GetComponentInParent<Slot>();

            if (slot == null) {
                DeselectObject();
            } else {
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
            }
        } else {
            DeselectObject();
        }
    }
    
    public float scrollTime = 0.2f;
    private float curScrollTime = 0;
    void ProcessScroll(float value) {
        if (curScrollTime <= 0f) {
            if (value > 0) {
                //print((activeIndex+1) % 3);
                var nextIndex = activeIndex;
                for (int i = 0; i < 2; i++) {
                    nextIndex = (nextIndex + 1) % 3;
                    if (activeSlot.myBuildings[nextIndex] != null) {
                        SelectObject(activeSlot, nextIndex);
                        activeActionSelection.SetUp(activeBuilding);
                    }
                }

                curScrollTime = scrollTime;
            }

            if (value < 0) {
                var nextIndex = activeIndex;
                for (int i = 0; i < 2; i++) {
                    nextIndex = (nextIndex + 2) % 3; // +2 actually makes us go -1 because modulo 3
                    if (activeSlot.myBuildings[nextIndex] != null) {
                        SelectObject(activeSlot, nextIndex);
                        activeActionSelection.SetUp(activeBuilding);
                    }
                }
                curScrollTime = scrollTime;
            }
        } else {
            curScrollTime -= Time.deltaTime;
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
                if (slot.myBuildings[1] != null) {
                    activeBuilding = slot.myBuildings[1];
                    index = 1;
                } else if (slot.myBuildings[0] != null) {
                    activeBuilding = slot.myBuildings[0];
                    index = 0;

                } else if (slot.myBuildings[2] != null) {
                    activeBuilding = slot.myBuildings[2];
                    index = 2;
                }
            }

            if (activeBuilding != null) {
                
                
                if (playerBuildingSkipOneTimeOverride) {
                    playerBuildingSkipOneTimeOverride = false;
                } else {
                    SelectBuilding(activeBuilding, true);
                }
            } 
        }
    }


    public void SelectBuilding(TrainBuilding building, bool isSelected) {
        isMouseOverSelectedObject = isSelected;
        if (building != null) {
            building.SetHighlightState(isSelected);
            
            var ranges = building.GetComponentsInChildren<RangeVisualizer>();

            for (int i = 0; i < ranges.Length; i++) {
                ranges[i].ChangeVisualizerStatus(isSelected);
            }
        }
    }
}