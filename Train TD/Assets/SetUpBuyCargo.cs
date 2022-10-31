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

    private void Start(){
        SetUpCargos();
    }

    private void OnEnable() {
        StarterUIController.s.OnLevelChanged.AddListener(SetUpCargos);
        StarterUIController.s.OnEnteredStarterUI.AddListener(SetUpCargos);
    }

    private void OnDisable() {
        StarterUIController.s.OnLevelChanged.RemoveListener(SetUpCargos);
    }

    void SetUpCargos() {
        ClearPreviousCargo();
        
        if (StarterUIController.s.levelSelected) {
            var playerStar = DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
            var targetLevel = StarterUIController.s.selectedLevelIndex;

            /*for (int i = 0; i < playerStar.outgoingConnectionCargoData[targetLevel].Count; i++) {
                var data = playerStar.outgoingConnectionCargoData[i];
                var button = Instantiate(cargoSellButtonPrefab, cargoParent).GetComponent<MiniGUI_StarterBuildingButton>();
                var cargo = GetCargoWithType(playerStar.city.cargosSold[Random.Range(0, playerStar.city.cargosSold.Length)]);

                button.SetUp(cargo.GetComponent<TrainBuilding>(), 3, data.cost, data.reward);
            }*/


            if (!playerStar.outgoingConnectionLevels[targetLevel].isEncounter) {
                var data = playerStar.outgoingConnectionCargoData[targetLevel];
                var button = Instantiate(cargoSellButtonPrefab, cargoParent).GetComponent<MiniGUI_StarterBuildingButton>();
                var cargo = GetCargoWithType(playerStar.city.cargosSold[Random.Range(0, playerStar.city.cargosSold.Length)]);

                button.SetUp(cargo.GetComponent<TrainBuilding>(), 20, data.cost, data.reward);
            } else {
                Instantiate(noCargoAvailableBecauseEncounter, cargoParent);
            }
        }
    }

    public void ClearPreviousCargo() {
        var buttons = cargoParent.GetComponentsInChildren<MiniGUI_StarterBuildingButton>();

        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].SellAllCargo();
            Destroy(buttons[i].gameObject);
        }
        
        cargoParent.DeleteAllChildren();


        var trainState = Train.s.GetTrainState();

        bool isDirty = false;
        for (int i = 0; i < trainState.myCarts.Count; i++) {
            for (int j = 0; j < trainState.myCarts[i].buildingStates.Length; j++) {
                var state = trainState.myCarts[i].buildingStates[j];

                for (int k = 0; k < cargoBuildings.Length; k++) {

                    if (state.uniqueName == cargoBuildings[k].GetComponent<TrainBuilding>().uniqueName) {
                        DataSaver.s.GetCurrentSave().currentRun.myResources.money += state.cargoCost;
                        state.EmptyState();
                        isDirty = true;
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
    }

    GameObject GetCargoWithType(CargoModule.CargoTypes type) {
        for (int i = 0; i < cargoBuildings.Length; i++) {
            if (cargoBuildings[i].GetComponent<CargoModule>().myCargoType == type) {
                return cargoBuildings[i];
            }
        }

        return null;
    }
}
