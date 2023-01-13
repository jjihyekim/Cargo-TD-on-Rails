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
    /*public TrainModuleHolder[] GetRewardScreenContent() {
        return GenerateModules(3);
    }*/


    public TrainModuleHolder[] act1ShopModules;
    public TrainModuleHolder[] act2ShopModules;
    public TrainModuleHolder[] act3ShopModules;
    
    TrainModuleHolder[] GenerateModules(int modCount) {
        TrainModuleHolder[] shopOptions = new TrainModuleHolder[modCount];

        TrainModuleHolder[] modules;
        switch (DataSaver.s.GetCurrentSave().currentRun.currentAct) {
            case 1:
                modules = act1ShopModules;
                break;
            case 2:
                modules = act2ShopModules;
                break;
            case 3:
                modules = act3ShopModules;
                break;
            default:
                modules = act3ShopModules;
                Debug.LogError($"Illegal Act Number {DataSaver.s.GetCurrentSave().currentRun.currentAct}");
                break;
        }

        for (int i = 0; i < modCount; i++) {
            shopOptions[i] = modules[Random.Range(0, modules.Length)].Copy();
        }

        return shopOptions;
    }
}
