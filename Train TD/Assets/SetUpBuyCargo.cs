using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SetUpBuyCargo : MonoBehaviour {
    public static SetUpBuyCargo s;

    private void Awake() {
        s = this;
    }

    public GameObject[] cargoBuildings;
    public GameObject cargoSellButtonPrefab;
    public Transform cargoParent;

    public GameObject noCargoAvailableBecauseEncounter;

    private void Start() {
        /*StarterUIController.s.OnLevelChanged.AddListener(SetUpCargos);
        StarterUIController.s.OnEnteredStarterUI.AddListener(SetUpCargos);*/
        SetUpCargos();
    }

    void SetUpCargos() {
        //ClearPreviousCargo();
        
        
        var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
        var button = Instantiate(cargoSellButtonPrefab, cargoParent).GetComponent<MiniGUI_StarterBuildingButton>();
        var cargo = GetCargoWithType(playerStar.city.cargosSold[Random.Range(0, playerStar.city.cargosSold.Length)]);

        button.SetUp(cargo.GetComponent<TrainBuilding>()/*, 20, data.cost, data.reward*/);
        
        /*if (StarterUIController.s.levelSelected) {
            var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
            var targetLevel = StarterUIController.s.selectedLevelIndex;

            if (targetLevel == -1) {
                return; // this means we are starting the tutorial level
            }
            
            if (!playerStar.outgoingConnectionLevels[targetLevel].isEncounter) {
                var data = playerStar.outgoingConnectionCargoData[targetLevel];
                var button = Instantiate(cargoSellButtonPrefab, cargoParent).GetComponent<MiniGUI_StarterBuildingButton>();
                var cargo = GetCargoWithType(playerStar.city.cargosSold[Random.Range(0, playerStar.city.cargosSold.Length)]);

                button.SetUp(cargo.GetComponent<TrainBuilding>(), 20, data.cost, data.reward);
            } else {
                Instantiate(noCargoAvailableBecauseEncounter, cargoParent);
            }
        }*/
    }

    /*public void ClearPreviousCargo() {
        var buttons = cargoParent.GetComponentsInChildren<MiniGUI_StarterBuildingButton>();

        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].SellAllCargo();
            Destroy(buttons[i].gameObject);
        }
        
        cargoParent.DeleteAllChildren();


        var trainState = Train.s.GetTrainState();

        bool isDirty = false;
        var skipCount = 0;
        for (int i = 0; i < trainState.myCarts.Count; i++) {
            for (int j = 0; j < trainState.myCarts[i].buildingStates.Length; j++) {
                
                
                var state = trainState.myCarts[i].buildingStates[j];
                
                for (int k = 0; k < cargoBuildings.Length; k++) {
                    var buildingScript = cargoBuildings[k].GetComponent<TrainBuilding>();
                    if (state.uniqueName == buildingScript.uniqueName) {
                        if (skipCount > 0) { // we dont add money for the next 3 duplicates but we still need to make the state empty.
                            skipCount -= 1;
                        } else {
                            MoneyController.s.ModifyResource(ResourceTypes.money, state.cargoCost);
                        }

                        state.EmptyState();
                        isDirty = true;
                        
                        if (buildingScript.occupiesEntireSlot && skipCount == 0) {
                            skipCount = 3;
                        } else if (j%4 == 0 && skipCount == 0) {//we skip the second top slot as it is just for forward/backward slot
                            skipCount = 1;
                        }
                        break;
                    }
                }
            }
        }

        if (isDirty) {
            Train.s.DrawTrain(trainState);
            DataSaver.s.GetCurrentSave().currentRun.myTrain = trainState;
            DataSaver.s.SaveActiveGame();
        }
    }*/

    GameObject GetCargoWithType(CargoModule.CargoTypes type) {
        for (int i = 0; i < cargoBuildings.Length; i++) {
            if (cargoBuildings[i].GetComponent<CargoModule>().myCargoType == type) {
                return cargoBuildings[i];
            }
        }

        return null;
    }
}
