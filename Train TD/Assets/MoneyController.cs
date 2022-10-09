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

    public int scraps = 50;
    public int maxScraps = 200;
    public int ammo = 50;
    public int maxAmmo = 100;

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
        if (SceneLoader.s.isLevelInProgress) {
            /*perSecondMoneyCounter += moneyPerSecond * Time.deltaTime;

            if (perSecondMoneyCounter > 0) {
                var addition = Mathf.FloorToInt(perSecondMoneyCounter);
                scraps += addition;
                perSecondMoneyCounter -= addition;
            }
            */
        }
    }


    public void SubtractScraps(int amount) {
        scraps -= amount;
        scraps = Mathf.Clamp(scraps, 0, maxScraps);
        myScraps.SetScrap(scraps);
    }

    public void AddScraps(int amount) {
        scraps += amount;
        scraps = Mathf.Clamp(scraps, 0, maxScraps);
        myScraps.SetScrap(scraps);
    }
    
    public void SubtractAmmo(int amount) {
        ammo -= amount;
        ammo = Mathf.Clamp(ammo, 0, maxAmmo);
        myAmmo.SetScrap(ammo);
    }

    public void AddAmmo(int amount) {
        ammo += amount;
        ammo = Mathf.Clamp(ammo, 0, maxAmmo);
        myAmmo.SetScrap(ammo);
    }
}
