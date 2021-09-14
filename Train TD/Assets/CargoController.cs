using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CargoController : MonoBehaviour {
    public static CargoController s;
    private void Awake() {
        s = this;
    }


    public int totalCargo;
    public int aliveCargo;

    
    public Transform cargoParent;
    public GameObject cargoDisplayPrefab;

    public List<GameObject> cargoDisplays = new List<GameObject>();
    
    public Color cargoActiveColor = Color.white;
    public Color cargoLostColor = Color.grey;

    public void UpdateBasedOnLevelData() {
        totalCargo = 0;
        var modules = LevelLoader.s.currentLevel.starterModules;
        for (int i = 0; i < modules.Length; i++) {
            var cargo = DataHolder.s.GetBuilding(modules[i].uniqueName).GetComponent<CargoModule>();

            if (cargo != null) {
                totalCargo += cargo.cargoSize * modules[i].count;
            }
        }

        aliveCargo = totalCargo;
    }

    public void AliveCargo(int size) {
        aliveCargo += size;
        UpdateStars();
        UpdateCargoAliveGraphics();
    }

    public void DeadCargo(int size) {
        aliveCargo -= size;

        if (LevelLoader.s.isLevelInProgress && totalCargo > 0 && aliveCargo == 0) {
            MissionLoseFinisher.s.MissionLost(true);
        }
        
        UpdateStars();
        UpdateCargoAliveGraphics();
    }

    void UpdateStars() {
        var starCount = 2;
        if (totalCargo - aliveCargo > 0) {
            starCount -= 1;
        }

        if (aliveCargo / (float)totalCargo < 0.5f) {
            starCount -= 1;
        }
        
        StarController.s.UpdateCargoStars(starCount);
    }
    
    void UpdateCargoAliveGraphics() {
        for (int i = 0; i < cargoDisplays.Count; i++) {
            if (i < aliveCargo) {
                cargoDisplays[i].GetComponent<Image>().color = cargoActiveColor;
            } else {
                cargoDisplays[i].GetComponent<Image>().color = cargoLostColor;
            }
        }
    }

}
