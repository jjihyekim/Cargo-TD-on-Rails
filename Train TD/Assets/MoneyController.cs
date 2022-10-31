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
    
    public float scrapPerSecond = 1f;
    public float ammoPerSecond = 0f;
    

    public void UpdateBasedOnLevelData() {
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
            scraps += amount;
            scraps = Mathf.Clamp(scraps, 0, maxScraps);
            myScraps.SetScrap(scraps);
        } else {
            var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
            currentRunMyResources.scraps += (int)amount;
            currentRunMyResources.scraps = Mathf.Clamp(currentRunMyResources.scraps, 0, currentRunMyResources.maxScraps);
        }
    }

    void ModifyAmmo(float amount) {
        if (SceneLoader.s.isLevelInProgress) {
            ammo += amount;
            ammo = Mathf.Clamp(ammo, 0, maxScraps);
            myAmmo.SetScrap(ammo);
        } else {
            var currentRunMyResources = DataSaver.s.GetCurrentSave().currentRun.myResources;
            currentRunMyResources.ammo += (int)amount;
            currentRunMyResources.ammo = Mathf.Clamp(currentRunMyResources.ammo, 0, currentRunMyResources.maxAmmo);
        }
    }


    public void SubtractScraps(float amount) {
        if (amount > 0) {
            ModifyScraps(-amount);
        }

        if (scraps <= 0) {
            SoundscapeController.s.PlayNoMoreResource(DataSaver.RunResources.Types.scraps);
        }
    }

    public void AddScraps(float amount) {
        if (amount > 0) {
            ModifyScraps(amount);
        }
    }
    
    public void SubtractAmmo(float amount) {
        if (amount > 0) {
            ModifyAmmo(-amount);
        }
        if (ammo <= 0) {
            SoundscapeController.s.PlayNoMoreResource(DataSaver.RunResources.Types.ammo);
        }
    }

    public void AddAmmo(float amount) {
        if (amount > 0) {
            ModifyAmmo(amount);
        }
    }
}
