using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FirstTimeTutorialController : MonoBehaviour {

    public LevelDataScriptable tutorialLevel;
    public CharacterDataScriptable tutorialCharacter;

    public bool tutorialEngaged = false;

    public Button mapButton;
    private void Awake() {
        tutorialUI.SetActive(false);
        if (PlayerPrefs.GetInt("finishedTutorial", 0) == 0) {
            DisableIfOpenedGuideBefore.shouldDisableBecauseOfTutorial = true;
            MapController.ApplyStarMap = false;
            Invoke(nameof(EngageFirstTimeTutorial),0.01f);
        } else {
            this.enabled = false;
        }
    }

    public void EngageFirstTimeTutorial() {
        if (!DataSaver.s.GetCurrentSave().isInARun || DataSaver.s.GetCurrentSave().currentRun.character.uniqueName != tutorialCharacter.myCharacter.uniqueName) {
            DataSaver.s.GetCurrentSave().currentRun = new DataSaver.RunState();
            DataSaver.s.GetCurrentSave().currentRun.SetCharacter(tutorialCharacter.myCharacter);
            DataSaver.s.GetCurrentSave().isInARun = true;

            MapController.s.GenerateStarMap();
            DataSaver.s.SaveActiveGame();
            SceneLoader.s.BackToStarterMenuHardLoad();
            return;
        }

        if(SceneLoader.s.isProfileMenu())
            ProfileSelectionMenu.s.StartGame();

        StarterUIController.s.SelectLevelAndStart(tutorialLevel.myData);
        tutorialEngaged = true;

        mapButton.interactable = false;

        StartCoroutine(TutorialProcess());
    }


    public bool cameraWASDMoved = false;
    public bool cameraZoomed = false;
    public bool cameraMiddleMouseRotated = false;
    public bool cameraRRotated = false;
    public bool cameraMovesetComplete = false;

    public bool makeMissionNeverEnd = true;
    private void Update() {
        if (tutorialEngaged) {
            if (makeMissionNeverEnd) {
                if (SpeedController.s.missionDistance - SpeedController.s.currentDistance < 50) {
                    SpeedController.s.IncreaseMissionEndDistance(50);
                }
            }

            if (SpeedController.s.fuel < 5) {
                MoneyController.s.ModifyResource(ResourceTypes.fuel, 5);
            }

            if (MoneyController.s.scraps < 50) {
                MoneyController.s.ModifyResource(ResourceTypes.scraps, 10);
            }

            if (!cameraMovesetComplete) {
                if (CameraController.s.moveAction.action.ReadValue<Vector2>().magnitude > 0) {
                    cameraWASDMoved = true;
                }
                if (Mathf.Abs(CameraController.s.zoomAction.action.ReadValue<float>()) > 0) {
                    cameraZoomed = true;
                }
                if (Mathf.Abs(CameraController.s.rotateCameraAction.action.ReadValue<float>()) > 0) {
                    cameraMiddleMouseRotated = true;
                }

                if (cameraWASDMoved && cameraZoomed /*&& cameraMiddleMouseRotated*/ && cameraRRotated) {
                    cameraMovesetComplete = true;
                    Invoke(nameof(DisableCameraMovesetHint), 2f);
                }
            }


            if (supplySpawned && !supplyDestroyed) {
                if (EnemyWavesController.s.waves.Count <= 0) {
                    supplyDestroyed = true;
                }
            }
            
            if (regularEnemySpawned && !regularEnemyDestroyed) {
                if (EnemyWavesController.s.waves.Count <= 0) {
                    regularEnemyDestroyed = true;
                }
            }
        }
    }

    void DisableCameraMovesetHint() {
        variousTutorialScreens[0].SetActive(false);
    }

    private void CamRotateActionDone(InputAction.CallbackContext obj) {
        CameraController.s.rotateAction.action.performed -= CamRotateActionDone;
        cameraRRotated = true;
    }


    public bool gunBuildingComplete = false;
    public bool currentlyBuildingGun = false;
    public bool cargoBuildingComplete = false;
    public bool currentlyBuildingCargo = false;
    public bool tryingToBuildGun = false;
    public bool tryingToBuildCargo = false;


    void GunMenuOpened() {
        if (!gunBuildingComplete) {
            variousTutorialScreens[1].SetActive(false);
            variousTutorialScreens[2].SetActive(true); // click on AVC
        }
    }

    void GunMenuClosed() {
        if (!currentlyBuildingGun) {
            variousTutorialScreens[1].SetActive(true);
            variousTutorialScreens[2].SetActive(false);
        }
    }

    void BuildingMenuOpened() {
        if (!cargoBuildingComplete) {
            variousTutorialScreens[4].SetActive(false);
            variousTutorialScreens[5].SetActive(true); // click on cargos
        }
    }

    void BuildingMenuClosed() {
        if (!currentlyBuildingCargo) {
            variousTutorialScreens[4].SetActive(true);
            variousTutorialScreens[5].SetActive(false);
        }
    }

    void BuildingDone() {
        var avcCount = 0;
        var cargoComboCount = 0;
        var cargoAmmoCount = 0;
        var slots = Train.s.GetComponentsInChildren<Slot>();
        for (int i = 0; i < slots.Length; i++) {
            for (int j = 0; j < slots[i].myBuildings.Length; j++) {
                var trainBuilding = slots[i].myBuildings[j];
                if (trainBuilding != null) {
                    if (trainBuilding.uniqueName == AVC.uniqueName) {
                        avcCount += 1;
                    } else if (trainBuilding.uniqueName == cargoCombo.uniqueName) {
                        cargoComboCount += 1;
                    }else if (trainBuilding.uniqueName == cargoAmmo.uniqueName) {
                        cargoAmmoCount += 1;
                    }
                }
            }
        }
        
        currentlyBuildingCargo = false;
        currentlyBuildingGun = false;
        
        if (avcCount >= 2) {
            gunBuildingComplete = true;
        } else {
            if (tryingToBuildGun && !gunBuildingComplete) {
                variousTutorialScreens[3].SetActive(false);
                GunMenuClosed();
            }
        }

        if (cargoComboCount > 0 && cargoAmmoCount > 0) {
            cargoBuildingComplete = true;
        } else {
            if (tryingToBuildCargo && !cargoBuildingComplete) {
                variousTutorialScreens[6].SetActive(false);
                BuildingMenuClosed();
            }
        }

    }

    public TrainBuilding AVC;
    public TrainBuilding cargoCombo;
    public TrainBuilding cargoAmmo;

    void OnPlayerIsBuilding() {
        var building = PlayerBuildingController.s.tempBuilding;

        if (building.uniqueName == AVC.uniqueName) {
            currentlyBuildingGun = true;
            
            variousTutorialScreens[1].SetActive(false);
            variousTutorialScreens[2].SetActive(false);
            variousTutorialScreens[3].SetActive(true); // build gun behind engine

            // let player build in these slots
            var backSlot = Train.s.carts[0].GetComponent<Cart>().backSlot;
            if (backSlot.myBuildings[2] != null && backSlot.myBuildings[2].uniqueName == DummyTrainModule.uniqueName) 
                backSlot.myBuildings[2] = null;
            
            if (backSlot.myBuildings[3] != null && backSlot.myBuildings[3].uniqueName == DummyTrainModule.uniqueName) 
                backSlot.myBuildings[3] = null;

        }else if (building.uniqueName == cargoCombo.uniqueName || building.uniqueName == cargoAmmo.uniqueName) {
            currentlyBuildingCargo = true;
            
            variousTutorialScreens[4].SetActive(false);
            variousTutorialScreens[5].SetActive(false);
            variousTutorialScreens[6].SetActive(true); // build cargo in the middle cart


            var backSlot = Train.s.carts[1].GetComponent<Cart>().backSlot;
            var frontSlot = Train.s.carts[1].GetComponent<Cart>().frontSlot;

            for (int i = 0; i < backSlot.myBuildings.Length; i++) {
                if (backSlot.myBuildings[i] != null && backSlot.myBuildings[i].uniqueName == DummyTrainModule.uniqueName) {
                    backSlot.myBuildings[i] = null;
                }
            }
            for (int i = 0; i < frontSlot.myBuildings.Length; i++) {
                if (frontSlot.myBuildings[i] != null && frontSlot.myBuildings[i].uniqueName == DummyTrainModule.uniqueName) {
                    frontSlot.myBuildings[i] = null;
                }
            }

        }
    }


    public MenuToggle gunMenuToggle;
    public MenuToggle buildingMenuToggle;
    public TrainBuilding DummyTrainModule;
    public string supplyEnemyName;
    public string regularEnemyName;

    public bool supplySpawned = false;
    public bool supplyDestroyed = false;
    
    public bool regularEnemySpawned = false;
    public bool regularEnemyDestroyed = false;

    void FillEmptyTrainSlotsWithDummy() {
        var slots = Train.s.GetComponentsInChildren<Slot>();
        for (int i = 0; i < slots.Length; i++) {
            for (int j = 0; j < slots[i].myBuildings.Length; j++) {
                if (slots[i].myBuildings[j] == null) {
                    slots[i].myBuildings[j] = DummyTrainModule;
                }
            }

            DummyTrainModule.mySlot = slots[i];
        }
    }


    public GameObject tutorialUI;
    public GameObject[] variousTutorialScreens;

    IEnumerator TutorialProcess() {
        FillEmptyTrainSlotsWithDummy();


        CameraController.s.rotateAction.action.performed += CamRotateActionDone;
        PlayerBuildingController.s.startBuildingEvent.AddListener(OnPlayerIsBuilding);
        PlayerBuildingController.s.completeBuildingEvent.AddListener(BuildingDone);

        for (int i = 0; i < variousTutorialScreens.Length; i++) {
            variousTutorialScreens[i].SetActive(false);
        }

        tutorialUI.SetActive(true);

        yield return new WaitForSeconds(5f);

        variousTutorialScreens[0].SetActive(true);

        yield return new WaitUntil(() => cameraMovesetComplete);
        yield return new WaitForSeconds(2.5f);

        variousTutorialScreens[1].SetActive(true); // click on gun build menu
        gunMenuToggle.PanelEnabledEvent.AddListener(GunMenuOpened);
        gunMenuToggle.PanelDisabledEvent.AddListener(GunMenuClosed);

        tryingToBuildGun = true;

        yield return new WaitUntil(() => gunBuildingComplete);

        tryingToBuildGun = false;

        variousTutorialScreens[1].SetActive(false);
        variousTutorialScreens[2].SetActive(false);
        variousTutorialScreens[3].SetActive(false);
        gunMenuToggle.PanelEnabledEvent.RemoveListener(GunMenuOpened);
        gunMenuToggle.PanelDisabledEvent.RemoveListener(GunMenuClosed);

        // send supply enemies

        variousTutorialScreens[7].SetActive(true); // wait till enemy arrives

        var enemyData = new EnemyOnPathData() {
            distanceOnPath = (int)SpeedController.s.currentDistance + 30,
            enemyIdentifier = new EnemyIdentifier() { enemyCount = 1, enemyUniqueName = supplyEnemyName },
            startMoving = false
        };

        EnemyWavesController.s.SpawnEnemy(enemyData);
        supplySpawned = true;

        yield return new WaitForSeconds(15f);
        
        variousTutorialScreens[7].SetActive(false);

        variousTutorialScreens[4].SetActive(true); // click on cargo build menu also say you have no ammo
        buildingMenuToggle.PanelEnabledEvent.AddListener(BuildingMenuOpened);
        buildingMenuToggle.PanelDisabledEvent.AddListener(BuildingMenuClosed);

        tryingToBuildCargo = true;

        yield return new WaitUntil(() => cargoBuildingComplete);
        
        
        tryingToBuildCargo = false;

        buildingMenuToggle.PanelEnabledEvent.RemoveListener(BuildingMenuOpened);
        buildingMenuToggle.PanelDisabledEvent.RemoveListener(BuildingMenuClosed);
        PlayerBuildingController.s.startBuildingEvent.RemoveListener(OnPlayerIsBuilding);
        PlayerBuildingController.s.completeBuildingEvent.RemoveListener(BuildingDone);

        variousTutorialScreens[4].SetActive(false);
        variousTutorialScreens[5].SetActive(false);
        variousTutorialScreens[6].SetActive(false);

        yield return new WaitForSeconds(3f);
        variousTutorialScreens[8].SetActive(true); // have some free ammo
        LevelReferences.s.SpawnResourceAtLocation(ResourceTypes.ammo, 50, Vector3.left);


        yield return new WaitForSeconds(8f);
        variousTutorialScreens[8].SetActive(false); 
        variousTutorialScreens[9].SetActive(true); // reload guns

        yield return new WaitUntil(() => supplyDestroyed);
        
        variousTutorialScreens[9].SetActive(false);
        variousTutorialScreens[10].SetActive(true); // nice now kill regular enemy
        
        var regularEnemyData = new EnemyOnPathData() {
            distanceOnPath = (int)SpeedController.s.currentDistance + 30,
            enemyIdentifier = new EnemyIdentifier() { enemyCount = 2, enemyUniqueName = regularEnemyName },
            startMoving = false
        };

        EnemyWavesController.s.SpawnEnemy(regularEnemyData);
        regularEnemySpawned = true;

        yield return new WaitUntil(() => regularEnemyDestroyed);
        
        
        variousTutorialScreens[10].SetActive(false); // nice now kill regular enemy
        variousTutorialScreens[11].SetActive(true); // repair train, and select next destination on the map once you arrive!

        makeMissionNeverEnd = false;

        yield return new WaitUntil(() => SpeedController.s.currentDistance + 1 >= SpeedController.s.missionDistance);

        TutorialComplete();
    }

    public CharacterDataScriptable defaultStartCharacter;

    void TutorialComplete() {
        mapButton.interactable = true;
        PlayerPrefs.SetInt("finishedTutorial", 1);
        DisableIfOpenedGuideBefore.shouldDisableBecauseOfTutorial = false;
        MapController.ApplyStarMap = true;
        CharacterSelector.s.SelectCharacter(defaultStartCharacter.myCharacter);
        MusicPlayer.s.SwapMusicTracksAndPlay(false);
    }
    
    public void SkipTutorial (){
        mapButton.interactable = true;
        PlayerPrefs.SetInt("finishedTutorial", 1);
        DisableIfOpenedGuideBefore.shouldDisableBecauseOfTutorial = false;
        MapController.ApplyStarMap = true;
        SettingsController.s.ResetRun();
        MusicPlayer.s.SwapMusicTracksAndPlay(false);
    }
}
