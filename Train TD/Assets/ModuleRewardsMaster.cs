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
            InitializeShop(currentRun);
        } 
        
        return currentRun.currentShopModules;
    }

    public float shopPriceVariance = 0.2f;
    
    private void InitializeShop(DataSaver.RunState currentRun) {
        currentRun.currentShopModules = GenerateModules(Random.Range(2, 3 + 1));

        var playerStar = currentRun.map.GetPlayerStar();
        currentRun.currentShopPrices = new List<SupplyPrice>();
        
        for (int i = 0; i < playerStar.city.prices.Length; i++) {
            var priceIndex = playerStar.city.prices[i].Copy();
            priceIndex.basePrice = (int)(priceIndex.basePrice * (1 + Random.Range(-shopPriceVariance, shopPriceVariance)));
            currentRun.currentShopPrices.Add(priceIndex);
        }

        currentRun.shopInitialized = true;
        DataSaver.s.SaveActiveGame();
    }

    public void ShopModuleBought(TrainModuleHolder holder) {
        var currentRun = DataSaver.s.GetCurrentSave().currentRun;
        var modules = new List<TrainModuleHolder>(currentRun.currentShopModules);
        modules.Remove(holder);
        currentRun.currentShopModules = modules.ToArray();
        DataSaver.s.SaveActiveGame();
    }

    
    // see UpgradesController.GetRandomLevelRewards
    public TrainModuleHolder[] GetRewardScreenContent() {
        return GenerateModules(3);
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
