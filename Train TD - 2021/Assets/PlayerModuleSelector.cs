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
    public bool isActionSelectionActive = false;


    public bool canSelectModules = true;
    public void EnableModuleSelecting() {
        canSelectModules = true;
    }

    public void DisableModuleSelecting() {
        canSelectModules = false;
        DeselectObject();
        CameraController.s.canZoom = true;
    }

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

        if (!activeActionSelection.IsMouseOverMenu()) { // we dont want to hide menu in case the player clicks on it
            // we dont want to hide menu in case the player clicks on it
            if (isActionSelectionActive) {
                HideModuleActionSelector();
                return;
            }
            
            if (activeBuilding != null) {
                activeActionSelection.gameObject.SetActive(true);
                activeActionSelection.SetUp(activeBuilding);
                isActionSelectionActive = true;
                
                if(PlayerPrefs.GetInt("SnapToPlayer", 1) == 1)
                    CameraController.s.SnapToTrainModule(activeBuilding);
                
            } else if (activeEnemy != null) {
                activeActionSelection.gameObject.SetActive(true);
                activeActionSelection.SetUp(activeEnemy);
                isActionSelectionActive = true;
                
                if(PlayerPrefs.GetInt("SnapToEnemies", 1) == 1)
                    CameraController.s.SnapToTransform(activeEnemy.transform);
            } else {
                HideModuleActionSelector();
            }
        }
    }

    public void ActivateActionDisplayOnTrainBuilding(TrainBuilding building) {
        if (activeBuilding != null) {
            SelectBuilding(activeBuilding, false);
        }
        activeBuilding = building;
        activeActionSelection.SetUp(activeBuilding);
        SelectBuilding(building, true);

        timerForNotCheckingForCursorMove = 0.5f;
    }

    public void HideModuleActionSelector() {
        activeActionSelection.gameObject.SetActive(false);
        isActionSelectionActive = false;
        
        CameraController.s.UnSnap();
    }

    public void ShowModuleActionSelector() {
        activeActionSelection.gameObject.SetActive(true);
        isActionSelectionActive = true;
    }

    public float timerForNotCheckingForCursorMove;
    private void Update() {
        if (canSelectModules ) {
            if (!isActionSelectionActive) {
                CastRayToSelectBuilding();
            }

            // Scroll to select different gun
            /*if (isMouseOverSelectedModule) {
                CameraController.s.canZoom = false;
                ProcessScroll(scroll.action.ReadValue<float>());
            } else {
                CameraController.s.canZoom = true;
            }*/

            if (isActionSelectionActive && timerForNotCheckingForCursorMove <= 0) { // hide the menu if the cursor moves away from it
                var mousePos = Mouse.current.position.ReadValue();
                var rectPos = RectTransformUtility.WorldToScreenPoint(OverlayCamsReference.s.uiCam, activeActionSelection.transform.position);
                
                //print(Mathf.Abs(mousePos.x - rectPos.x));
                if (Mathf.Abs(mousePos.x - rectPos.x) > 380) {
                    HideModuleActionSelector();
                }
                if (Mathf.Abs(mousePos.y - rectPos.y) > 330) {
                    HideModuleActionSelector();
                }
            } else {
                timerForNotCheckingForCursorMove -= Time.deltaTime;
            }
        }
    }
    


    private Slot activeSlot;
    private int activeIndex;
    private int lastRaycastIndex;
    private TrainBuilding activeBuilding;

    private EnemyHealth activeEnemy;

    public bool playerBuildingDisableOverride = false;
    public bool playerBuildingSkipOneTimeOverride = false;

    public bool isMouseOverSelectedModule = false;

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

        if (Physics.Raycast(ray, out hit, 100f, LevelReferences.s.buildingLayer | LevelReferences.s.enemyLayer)) {
            var slot = hit.collider.gameObject.GetComponentInParent<Slot>();
            var enemy = hit.collider.gameObject.GetComponentInParent<EnemyHealth>();

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
            } else if (enemy != null) {
                DeselectObject();
                SelectEnemy(enemy);
            } else {
                DeselectObject();
            }
        } else {
            DeselectObject();
        }
    }

    public float scrollTime = 0.2f;
    private float curScrollTime = 0;
    void ProcessScroll(float value) {
        if (curScrollTime <= 0f) {
            if (activeSlot != null) {
                var slotCount = activeSlot.myBuildings.Length;
                
                if (value > 0) {
                    //print((activeIndex+1) % 3);
                    
                    var nextIndex = activeIndex;
                    for (int i = 0; i < slotCount-1; i++) {
                        nextIndex = (nextIndex + 1) % slotCount;
                        if (activeSlot.myBuildings[nextIndex] != null) {
                            SelectObject(activeSlot, nextIndex);
                            activeActionSelection.SetUp(activeBuilding);
                            break;
                        }
                    }

                    curScrollTime = scrollTime;
                }

                if (value < 0) {
                    var nextIndex = activeIndex;
                    for (int i = 0; i < slotCount-1; i++) {
                        nextIndex = (nextIndex + (slotCount-1)) % slotCount; // +2 actually makes us go -1 because modulo 3
                        if (activeSlot.myBuildings[nextIndex] != null) {
                            SelectObject(activeSlot, nextIndex);
                            activeActionSelection.SetUp(activeBuilding);
                            break;
                        }
                    }

                    curScrollTime = scrollTime;
                }
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
        if (activeEnemy != null) {
            activeEnemy.SetHighlightState(false);
            activeEnemy = null;
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
                if (playerBuildingSkipOneTimeOverride) {
                    playerBuildingSkipOneTimeOverride = false;
                } else {
                    SelectBuilding(activeBuilding, true);
                }
            } 
        }
    }
    
    public void SelectBuilding(TrainBuilding building, bool isSelected) {
        isMouseOverSelectedModule = isSelected;
        if (building != null) {
            building.SetHighlightState(isSelected);
            
            var ranges = building.GetComponentsInChildren<RangeVisualizer>();

            for (int i = 0; i < ranges.Length; i++) {
                ranges[i].ChangeVisualizerEdgeShowState(isSelected);
            }
        }
    }
    
    void SelectEnemy(EnemyHealth enemyHealth) {
        isMouseOverSelectedModule = false;
        if (enemyHealth != activeEnemy) {
            if (activeEnemy != null) {
                enemyHealth.SetHighlightState(false);
            }

            if (enemyHealth != null) {
                enemyHealth.SetHighlightState(true);
                activeEnemy = enemyHealth;
            }
        }
    }
}