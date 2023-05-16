using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FirstTimeTutorialController : MonoBehaviour {
    public static FirstTimeTutorialController s;

    private DataSaver.TutorialProgress _progress => DataSaver.s.GetCurrentSave().tutorialProgress;
    
    public bool tutorialEngaged = false;
    public GameObject tutorialUI;

    [Space]
    public GameObject cameraHint; 
    
    public bool cameraWASDMoved = false;
    public bool cameraZoomed = false;
    public bool cameraRRotated = false;
    
    [Space]
    public GameObject getCargoHint;
    public GameObject cargoIsHere;
    public GameObject activateMoveMode;
    public GameObject putOnTheTrain;
    
    [Space] 
    public GameObject getScrapsHint;
    public GameObject activateScrappingMode;


    [Space] 
    public GameObject openMapHint;
    public GameObject mapHere;
    public GameObject selectACity;


    [Space] 
    public GameObject goGoGoHint;


    [Space] 
    public GameObject lookAtTheRadarHint;
    
    
    [Space] 
    public GameObject pressShiftHint;
    public float shiftHoldTime = 0;
    
    
    [Space] 
    public GameObject useRepairTool;
    public GameObject useReloadTool;
    
    
    [Space] 
    public GameObject directControlHint;
    
    
    [Space] 
    public GameObject getRewardsHint;
    
    
    [Space] 
    public GameObject buildYourNewThingsHint;
    private void Awake() {
        s = this;
        tutorialUI.SetActive(false);
        enabled = false;
    }

    public void TutorialCheck() {
        return;
        if (!_progress.tutorialDone && !tutorialEngaged) {
            EngageFirstTimeTutorial();
        }
    }

    public void ReDoTutorial() {
        //TutorialComplete();
        DataSaver.s.GetCurrentSave().isInARun = false;
        DataSaver.s.GetCurrentSave().tutorialProgress = new DataSaver.TutorialProgress();
        //ShopStateController.s.BackToMainMenu();
        SceneLoader.s.ForceReloadScene();
    }

    void EngageFirstTimeTutorial() {
        tutorialEngaged = true;
        this.enabled = true;

        DataSaver.s.GetCurrentSave().tutorialProgress = new DataSaver.TutorialProgress();
        
        tutorialUI.SetActive(true);
        
        cameraHint.SetActive(false);
        getCargoHint.SetActive(false);
        getScrapsHint.SetActive(false);
        openMapHint.SetActive(false);
        goGoGoHint.SetActive(false);
        lookAtTheRadarHint.SetActive(false);
        pressShiftHint.SetActive(false);
        useRepairTool.SetActive(false);
        useReloadTool.SetActive(false);
        directControlHint.SetActive(false);
        getRewardsHint.SetActive(false);
        buildYourNewThingsHint.SetActive(false);
        
        //PlayerBuildingController.s.completeBuildingEvent.AddListener(OnPlayerBuildOnTrain);
        PlayStateMaster.s.OnCharacterSelected.AddListener(ShowCameraControls);
        if (DataSaver.s.GetCurrentSave().isInARun) {
            ShowCameraControls();
        }

        Train.s.trainUpdated.AddListener(GetTrainStuffBeforeLevelBegins);
    }


    void ShowCameraControls() {
        if (!_progress.cameraDone) {
            CameraController.s.rotateAction.action.performed += CameraRRotated;
            cameraHint.SetActive(true);
        } else {
            cameraHint.SetActive(false);
            ShowGetCargoControls();
        }
    }

    void ShowGetCargoControls() {
        if (!_progress.cargoPutOnTrain) {
            getCargoHint.SetActive(true);
        } else {
            getCargoHint.SetActive(false);
            ShowSellScrap();
        }
    }

    void ShowSellScrap() {
        if (!_progress.scrapsScrapped) {
            getScrapsHint.SetActive(true);
        } else {
            getScrapsHint.SetActive(false);
            OpenMap();
        }
    }

    void OpenMap() {
        if (!_progress.mapTargetSelected) {
            openMapHint.SetActive(true);
        } else {
            openMapHint.SetActive(false);
            ShowGoGoGo();
        }
    }

    void ShowGoGoGo() {
        if (!_progress.levelStarted) {
            goGoGoHint.SetActive(true);
        } else {
            goGoGoHint.SetActive(false);
        }
    }

    void GetTrainStuffBeforeLevelBegins() {
        Invoke(nameof(OneFrameLater),1f);
    }

    void OneFrameLater() {
        /*var sellModules = Train.s.GetComponentsInChildren<SellAction>();
        for (int i = 0; i < sellModules.Length; i++) {
            sellModules[i].sellEvent.AddListener(PlayerSoldSomethingOnTrain);
        }*/
    }

    private ModuleHealth[] _healths;
    private ModuleAmmo[] _ammos;
    void GetTrainStuff() {
        _healths = Train.s.GetComponentsInChildren<ModuleHealth>();
        _ammos = Train.s.GetComponentsInChildren<ModuleAmmo>();
    }
    
    void LookAtTheRadar() {
        lookAtTheRadarHint.SetActive(true);
        Invoke(nameof(PressShift), 5f);
    }

    void PressShift() {
        lookAtTheRadarHint.SetActive(false);
        pressShiftHint.SetActive(true);
    }

    void CameraRRotated(InputAction.CallbackContext obj) {
        cameraRRotated = true;
        CameraController.s.rotateAction.action.performed -= CameraRRotated;
    }

    private ModuleHealth cargoHealth;

    public GameObject soldSomethingOnTrain;
    void PlayerSoldSomethingOnTrain() {
        soldSomethingOnTrain.SetActive(true);
        CancelInvoke(nameof(DisableSoldOnTrain));
        Invoke(nameof(DisableSoldOnTrain), 20f);
    }

    void DisableSoldOnTrain() {
        soldSomethingOnTrain.SetActive(false);
    }

    private bool addedListeners = false;

    void OnRepaired() {
        _progress.repair += 1;
    }

    void OnReloaded() {
        _progress.reload += 1;
    }
    private void Update() {
        if (tutorialEngaged) {
            if (!_progress.cameraDone) {
                if (CameraController.s.moveAction.action.ReadValue<Vector2>().magnitude > 0) {
                    cameraWASDMoved = true;
                }
                if (Mathf.Abs(CameraController.s.zoomAction.action.ReadValue<float>()) > 0) {
                    cameraZoomed = true;
                }

                if (cameraWASDMoved && cameraZoomed && cameraRRotated) {
                    _progress.cameraDone = true;
                    Invoke(nameof(DisableCameraMovesetHint), 2f);
                }
            }


            if (!_progress.cargoPutOnTrain && _progress.cameraDone) {
                /*if (PlayerActionsController.s.currentMode != PlayerActionsController.ActionModes.shopMove && !PlayerBuildingController.s.isBuilding) {
                    activateMoveMode.SetActive(true);
                } else {
                    activateMoveMode.SetActive(false);
                }*/

                /*if (PlayerBuildingController.s.isBuilding) {
                    activateMoveMode.SetActive(false);
                    putOnTheTrain.SetActive(true);
                } else {
                    putOnTheTrain.SetActive(false);
                }*/
            }

            if (waitToShowScrap) {
                /*if (PlayerActionsController.s.currentMode != PlayerActionsController.ActionModes.shopMove && !PlayerBuildingController.s.isBuilding) {
                    ShowSellScrap();
                    waitToShowScrap = false;
                }*/
            }


            if (!_progress.scrapsScrapped && _progress.cargoPutOnTrain && _progress.cameraDone && !waitToShowScrap) {
                /*if (PlayerActionsController.s.currentMode != PlayerActionsController.ActionModes.shopSell) {
                    activateScrappingMode.SetActive(true);
                } else {
                    activateScrappingMode.SetActive(false);
                }*/
                
                if (MoneyController.s.scraps > 0) {
                    getScrapsHint.SetActive(false);
                    _progress.scrapsScrapped = true;
                    OpenMap();
                }
            }

            if (!_progress.mapTargetSelected && _progress.scrapsScrapped && _progress.cargoPutOnTrain) {
                if (PlayStateMaster.s.IsLevelSelected()) {
                    openMapHint.SetActive(false);
                    _progress.mapTargetSelected = true;
                    ShowGoGoGo();
                }

                if (WorldMapCreator.s.worldMapOpen) {
                    selectACity.SetActive(true);
                    mapHere.SetActive(false);
                } else {
                    selectACity.SetActive(false);
                    mapHere.SetActive(true);
                }
            }

            if (!_progress.levelStarted && _progress.mapTargetSelected && _progress.scrapsScrapped && _progress.cargoPutOnTrain) {
                if (PlayStateMaster.s.isCombatInProgress()) {
                    _progress.levelStarted = true;
                    goGoGoHint.SetActive(false);
                    GetTrainStuff();
                    
                    if(!_progress.shiftToGoFast)
                        Invoke(nameof(LookAtTheRadar), 5f);
                    /*if(!_progress.powerup)
                        PlayerActionsController.s.OnGetPowerUp.AddListener(OnPowerUpGet);*/
                }
            }

            if (PlayStateMaster.s.isCombatInProgress()){
                if (!addedListeners) {
                    /*PlayerActionsController.s.OnRepaired.AddListener(OnRepaired);
                    PlayerActionsController.s.OnReloaded.AddListener(OnReloaded);*/
                    addedListeners = true;
                }
                
                if (pressShiftHint.activeSelf) {
                    if (shiftHoldTime < 1f) {
                        if (TimeController.s.fastForwardKey.action.ReadValue<float>() > 0f) {
                            shiftHoldTime += Time.deltaTime;
                        }
                    } else {
                        _progress.shiftToGoFast = true;
                        pressShiftHint.SetActive(false);
                    }
                }


                bool isHealthLow = false;
                if (_progress.repair < 10) {
                    for (int i = 0; i < _healths.Length; i++) {
                        if (_healths[i].GetHealthPercent() < 0.5f) {
                            isHealthLow = true;
                            break;
                        }
                    }
                }

                useRepairTool.SetActive(isHealthLow);

                bool isAmmoLow = false;
                if (_progress.reload < 2) {
                    for (int i = 0; i < _ammos.Length; i++) {
                        if (_ammos[i].curAmmo < 2) {
                            isAmmoLow = true;
                            break;
                        }
                    }
                }

                useReloadTool.SetActive(isAmmoLow);
                
                if (!_progress.directControl) {
                    if (SpeedController.s.currentDistance > SpeedController.s.missionDistance/2f) {
                        directControlHint.SetActive(true);
                    } else {
                        directControlHint.SetActive(false);
                    }

                    if (DirectControlMaster.s.directControlInProgress || _progress.directControl) {
                        _progress.directControl = true;
                        directControlHint.SetActive(false);
                    }
                }
            }

            if (PlayStateMaster.s.isCombatFinished()) {
                useReloadTool.SetActive(false);
                useRepairTool.SetActive(false);
                directControlHint.SetActive(false);
                powerUpHint.SetActive(false);
                _progress.levelFinishedOnce = true;
                if (!_progress.getRewards && MissionWinFinisher.s.isWon) {
                    getRewardsHint.SetActive(true);

                    /*var remainingRewards = MissionWinFinisher.s.unclaimedRewardCount;
                    if (remainingRewards <= 0) {
                        _progress.getRewards = true;
                    }*/
                }
            }

            if (PlayStateMaster.s.isShop() && _progress.levelFinishedOnce) {
                if (!_progress.putTheNewStuff) {
                    buildYourNewThingsHint.SetActive(true);
                }
            }
        }
    }

    [Space] 
    public GameObject powerUpHint;
    void OnPowerUpGet() {
        //PlayerActionsController.s.OnGetPowerUp.RemoveListener(OnPowerUpGet);
        _progress.powerup = true;
        powerUpHint.SetActive(true);
        Invoke(nameof(HidePowerup), 10f);
    }

    void HidePowerup() {
        powerUpHint.SetActive(false);
    }

    private bool waitToShowScrap = false;
    void OnPlayerBuildOnTrain() {
        if (!_progress.cargoPutOnTrain) {
            var cargoModule = Train.s.GetComponentsInChildren<CargoModule>();

            if (cargoModule.Length > 0) {
                getCargoHint.SetActive(false);
                _progress.cargoPutOnTrain = true;
                waitToShowScrap = true;

                for (int i = 0; i < cargoModule.Length; i++) {
                    cargoModule[i].GetComponent<ModuleHealth>().invincibleTutorial = true;
                }
            }
        }

        if (!_progress.putTheNewStuff && _progress.levelFinishedOnce) {
            _progress.putTheNewStuff = true;
            TutorialComplete();
        }
    }

    void DisableCameraMovesetHint() {
        cameraHint.SetActive(false);
        ShowGetCargoControls();
    }

    void TutorialComplete() {
        tutorialEngaged = false;
        DataSaver.s.GetCurrentSave().tutorialProgress.tutorialDone = true;
        tutorialUI.SetActive(false);
        /*if (addedListeners) {
            PlayerActionsController.s.OnRepaired.RemoveListener(OnRepaired);
            PlayerActionsController.s.OnReloaded.RemoveListener(OnReloaded);
        }*/
    }
    
    public void SkipTutorial (){
        if (tutorialEngaged) {
            TutorialComplete();
        }
    }

    public void StopTutorial() {
        if (tutorialEngaged) {
            tutorialUI.SetActive(false);
            tutorialEngaged = false;
        }
    }
}
