using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility;

public class MiniGUI_HealthBar : MonoBehaviour{

    public IHealth myHealth;
    public ModuleAmmo myModuleAmmo;
    private bool isAmmoBar = false;

    public Slider slider;
    public Image filler;
    public Color playerColor = Color.green;
    public Color armoredEnemyColor = Color.yellow;
    public Color enemyColor = Color.red;
    
    public Color bulletAmmoColor = Color.yellow;
    public Color fuelAmmoColor = Color.red;
    
    
    public Slider ammoSlider;
    public Image ammoFiller;

    public bool isPlayer;

    public static bool showHealthBars = true;
    public static bool showAmmoBars = true;

    
    public void SetUp(ModuleHealth moduleHealth, ModuleAmmo moduleAmmo) {
        isPlayer = true;
        myHealth = moduleHealth;
        myModuleAmmo = moduleAmmo;
        if (myModuleAmmo != null) {
            isAmmoBar = true;

            var refillType = myModuleAmmo.GetComponent<ReloadAction>().myType;
            switch (refillType) {
                case ResourceTypes.ammo:
                    ammoFiller.color = bulletAmmoColor;
                    break;
                case ResourceTypes.fuel:
                    ammoFiller.color = fuelAmmoColor;
                    break;
            }
        }
        ammoSlider.gameObject.SetActive(isAmmoBar);

        if (moduleHealth.GetComponent<TrainBuilding>()) {
            GetComponent<UIElementFollowWorldTarget>().SetUp(moduleHealth.GetComponent<TrainBuilding>().GetUITargetTransform(true));
        } else {
            GetComponent<UIElementFollowWorldTarget>().SetUp(moduleHealth.GetComponent<Cart>().uiTargetTransform);
        }

        Update();
    }
    
    public void SetUp(EnemyHealth enemyHealth) {
        isPlayer = false;
        myHealth = enemyHealth;
        ammoSlider.gameObject.SetActive(false);
        
        GetComponent<UIElementFollowWorldTarget>().SetUp(enemyHealth.uiTransform);

        
        Update();
    }

    private void Update() {
        if (showHealthBars) {
            SetHealthBarValue();
        } else {
            slider.gameObject.SetActive(false);
        }

        if (showAmmoBars) {
            if (isAmmoBar)
                SetAmmoBarValue();
        } else {
            ammoSlider.gameObject.SetActive(false);
        }
    }

    void SetHealthBarValue() {
        var percent = myHealth.GetHealthPercent();
        percent = Mathf.Clamp(percent, 0, 1f);

        slider.value = percent;

        if (percent >= 1f) {
            if (slider.gameObject.activeSelf)
                slider.gameObject.SetActive(false);
        } else {
            if (!slider.gameObject.activeSelf)
                slider.gameObject.SetActive(true);
        }

        if (isPlayer) {
            //filler.color = Color.Lerp(deadColor, healthyColor, percent);
            filler.color = playerColor;
        } else {
            if (myHealth.HasArmor()) {
                filler.color = armoredEnemyColor;
            } else {
                filler.color = enemyColor;
            }
        }
    }

    void SetAmmoBarValue() {
        if (isAmmoBar) {
            var percent = (float)myModuleAmmo.curAmmo / myModuleAmmo.maxAmmo;
            percent = Mathf.Clamp(percent, 0, 1f);

            
            ammoSlider.value = percent;

            if(!ammoSlider.gameObject.activeSelf)
                ammoSlider.gameObject.SetActive(true);
        }
    }
}
