using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CargoModule : MonoBehaviour {

    public int cargoSize = 1;

    public int moneyReward = 100;

    private void OnEnable() {
        if(LevelLoader.s.isLevelInProgress)
	        CargoController.s.AliveCargo(cargoSize);
    }

    private void OnDisable() {
        if(LevelLoader.s.isLevelInProgress && CargoController.s != null) 
            CargoController.s.DeadCargo(cargoSize);
    }
}
