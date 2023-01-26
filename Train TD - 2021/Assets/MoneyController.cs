using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyController : MonoBehaviour {
    public static MoneyController s;

    private void Awake() {
        s = this;
    }


    public ScrapBoxScript myScraps;
    public ScrapBoxScript myAmmo;

    
    public float scraps { get; private set; }
    public int maxScraps = 200;
    public float ammo { get; private set; }
    public int maxAmmo = 100;

    public float money {
        get {
            return DataSaver.s.GetCurrentSave().currentRun.myResources.money;
        }
    }

    public float fuel {
        get {
            if(SceneLoader.s.isLevelInProgress)
                return SpeedController.s.fuel;
            else {
                return DataSaver.s.GetCurrentSave().currentRun.myResources.fuel;
            }
        }
    }

    public float maxFuel {
        get {
            return SpeedController.s.maxFuel;
        }
    }
    
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
        }
        
        if (ammo <= 0) {
            SoundscapeController.s.PlayNoMoreResource(ResourceTypes.ammo);
        }
    }

    public bool HasResource(ResourceTypes type, float amount) {
        switch (type) {
            case ResourceTypes.scraps:
                return scraps >= amount;
            case ResourceTypes.ammo:
                return ammo >= amount;
            case ResourceTypes.fuel:
                return SpeedController.s.fuel >= amount;
            case ResourceTypes.money:
                var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
                return currentRunMyResources.money >= amount;
            default:
                return false;
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
                    SpeedController.s.ModifyFuel(amount);
                    break;
                case ResourceTypes.money:
                    var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
                    currentRunMyResources.money += (int)amount;
                    break;
            }

            if (!SceneLoader.s.isLevelInProgress)
                DataSaver.s.SaveActiveGame();
        }
    }

    public void ApplyStorageAmounts(int _maxScraps, int _maxAmmo) {
        maxScraps = _maxScraps;
        myScraps.SetMaxScrap(maxScraps);
        maxAmmo = _maxAmmo;
        myAmmo.SetMaxScrap(maxAmmo);
    }
}
