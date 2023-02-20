using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MoneyController : MonoBehaviour {
    public static MoneyController s;

    private void Awake() {
        s = this;
    }


    public ScrapBoxScript myScraps;
    public ScrapBoxScript myAmmo;
    public ScrapBoxScript myFuel;

    
    public float scraps { get; private set; }
    public int maxScraps = 200;
    public float ammo { get; private set; }
    public int maxAmmo = 100;

    public float money {
        get {
            return DataSaver.s.GetCurrentSave().currentRun.myResources.money;
        }
    }

    public float fuel { get; private set; }
    public float maxFuel = 50;
    
    public float scrapPerSecond = 1f;
    public float ammoPerSecond = 0f;

    public void UpdateBasedOnLevelData() {
        if (DataSaver.s.GetCurrentSave().isInARun) {
            var resources = DataSaver.s.GetCurrentSave().currentRun.myResources;

            scraps = resources.scraps;
            maxScraps = resources.maxScraps;
            myScraps.SetMaxScrap(resources.maxScraps);
            myScraps.SetScrap(scraps);

            ammo = resources.ammo;
            maxAmmo = resources.maxAmmo;
            myAmmo.SetMaxScrap(resources.maxAmmo);
            myAmmo.SetScrap(resources.ammo);
            
            
            fuel = resources.fuel;
            maxFuel = resources.maxFuel;
            myFuel.SetMaxScrap(resources.maxFuel);
            myFuel.SetScrap(resources.fuel);
        }
    }


    private void Update() {
        if (DataSaver.s.GetCurrentSave().isInARun) {
            if (SceneLoader.s.isLevelInProgress) {
                if (scrapPerSecond > 0) {
                    scraps += scrapPerSecond * Time.deltaTime;
                    scraps = Mathf.Clamp(scraps, 0, maxScraps);
                    myScraps.SetScrap(scraps);
                }

                if (ammoPerSecond > 0) {
                    ammo += ammoPerSecond * Time.deltaTime;
                    ammo = Mathf.Clamp(ammo, 0, maxAmmo);
                    myScraps.SetScrap(scraps);
                }
            } else {
                scraps = DataSaver.s.GetCurrentSave().currentRun.myResources.scraps;
                ammo = DataSaver.s.GetCurrentSave().currentRun.myResources.ammo;
                fuel = DataSaver.s.GetCurrentSave().currentRun.myResources.fuel;
            }
        }
    }

    void ModifyScraps(float amount) {
        if (SceneLoader.s.isLevelInProgress) {
            if (scraps <= maxScraps) {
                scraps += amount;
                scraps = Mathf.Clamp(scraps, 0, maxScraps);
            } else {
                scraps += amount;
            }

            myScraps.SetScrap(scraps);
        } else {
            var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
            if (currentRunMyResources.scraps <= currentRunMyResources.maxScraps) {
                currentRunMyResources.scraps += (int)amount;
                currentRunMyResources.scraps = Mathf.Clamp(currentRunMyResources.scraps, 0, currentRunMyResources.maxScraps);
                scraps = currentRunMyResources.scraps;
            }
            
            DataSaver.s.SaveActiveGame();
        }
        
        if (scraps <= 0) {
            SoundscapeController.s.PlayNoMoreResource(ResourceTypes.scraps);
        }
    }

    void ModifyAmmo(float amount) {
        if (SceneLoader.s.isLevelInProgress) {
            if (ammo <= maxAmmo) {
                ammo += amount;
                ammo = Mathf.Clamp(ammo, 0, maxAmmo);
            }else {
                ammo += amount;
            }
            
            myAmmo.SetScrap(ammo);
        } else {
            var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
            if (currentRunMyResources.ammo <= currentRunMyResources.maxAmmo) {
                currentRunMyResources.ammo += (int)amount;
                currentRunMyResources.ammo = Mathf.Clamp(currentRunMyResources.ammo, 0, currentRunMyResources.maxAmmo);
            }

            ammo = currentRunMyResources.ammo;
            
            DataSaver.s.SaveActiveGame();
        }
        
        if (ammo <= 0) {
            SoundscapeController.s.PlayNoMoreResource(ResourceTypes.ammo);
        }
    }
    
    public void ModifyFuel(float amount) {
        if (SceneLoader.s.isLevelInProgress) {
            if (fuel <= maxFuel) {
                fuel += amount;
                fuel = Mathf.Clamp(fuel, 0, maxFuel);
            } else {
                fuel += amount;
            }
            
            myFuel.SetScrap(fuel);
        } else {
            var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
            if (currentRunMyResources.fuel <= currentRunMyResources.maxFuel) {
                currentRunMyResources.fuel += (int)amount;
                currentRunMyResources.fuel = Mathf.Clamp(currentRunMyResources.fuel, 0, currentRunMyResources.maxFuel);
            }

            fuel = currentRunMyResources.fuel;
            
            DataSaver.s.SaveActiveGame();
        }
        
        if (fuel <= 0) {
            SoundscapeController.s.PlayNoMoreResource(ResourceTypes.fuel);
        }
    }

    public bool HasResource(ResourceTypes type, float amount) {
        switch (type) {
            case ResourceTypes.scraps:
                return scraps >= amount;
            case ResourceTypes.ammo:
                return ammo >= amount;
            case ResourceTypes.fuel:
                return fuel >= amount;
            case ResourceTypes.money:
                var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
                return currentRunMyResources.money >= amount;
            default:
                return false;
        }
    }
    
    public float GetAmountPossibleToPay(ResourceTypes type, float amount) {
        switch (type) {
            case ResourceTypes.scraps:
                return Mathf.Min(amount,scraps);
            case ResourceTypes.ammo:
                return Mathf.Min(amount,ammo);
            case ResourceTypes.fuel:
                return Mathf.Min(amount,fuel);
            case ResourceTypes.money:
                var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
                return Mathf.Min(amount,currentRunMyResources.money);
            default:
                return 0;
        }
    }

    public void ModifyResource(ResourceTypes type, float amount) {
        if (amount != 0) {
            switch (type) {
                case ResourceTypes.scraps:
                    ModifyScraps(amount);
                    break;
                case ResourceTypes.ammo:
                    ModifyAmmo(amount);
                    break;
                case ResourceTypes.fuel:
                    ModifyFuel(amount);
                    break;
                case ResourceTypes.money:
                    var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
                    currentRunMyResources.money += (int)amount;
                    DataSaver.s.SaveActiveGame();
                    break;
            }

            if (!SceneLoader.s.isLevelInProgress)
                DataSaver.s.SaveActiveGame();
        }
    }

    public void ApplyStorageAmounts(int _maxScraps, int _maxAmmo, int _maxFuel) {
        maxScraps = _maxScraps;
        myScraps.SetMaxScrap(maxScraps);
        maxAmmo = _maxAmmo;
        myAmmo.SetMaxScrap(maxAmmo);
        maxFuel = _maxFuel;
        myFuel.SetMaxScrap(maxFuel);
    }


    [Button]
    void DebugAddResource(ResourceTypes type, int amount) {
        ModifyResource(type, amount);
    }
}
