using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModuleRewardsMaster : MonoBehaviour {
    public static ModuleRewardsMaster s;

    private void Awake() {
        s = this;
    }

    public TrainModuleHolder[] GetShopContent() {
        var currentRun = DataSaver.s.GetCurrentSave().currentRun;
        if (!currentRun.shopInitialized) {
            currentRun.currentShopModules = GenerateModules(Random.Range(2, 3 + 1));
            currentRun.shopInitialized = true;
            DataSaver.s.SaveActiveGame();
        } 
        
        return currentRun.currentShopModules;
    }

    public void ShopModuleBought(TrainModuleHolder holder) {
        var currentRun = DataSaver.s.GetCurrentSave().currentRun;
        var modules = new List<TrainModuleHolder>(currentRun.currentShopModules);
        modules.Remove(holder);
        currentRun.currentShopModules = modules.ToArray();
        DataSaver.s.SaveActiveGame();
    }

    public TrainModuleHolder[] GetRewardScreenContent() {
        return GenerateModules(2);
    }


    public TrainModuleHolder[] possibleRewards;
    
    TrainModuleHolder[] GenerateModules(int modCount) {
        TrainModuleHolder[] shopOptions = new TrainModuleHolder[modCount];

        for (int i = 0; i < modCount; i++) {
            shopOptions[i] = possibleRewards[Random.Range(0,possibleRewards.Length)].Copy();
        }

        return shopOptions;
    }
}
