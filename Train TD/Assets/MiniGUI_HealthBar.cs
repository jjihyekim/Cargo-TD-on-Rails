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
    
    
    public Slider ammoSlider;

    public bool isPlayer;

    
    public void SetUp(ModuleHealth moduleHealth, ModuleAmmo moduleAmmo) {
        isPlayer = true;
        myHealth = moduleHealth;
        myModuleAmmo = moduleAmmo;
        if (myModuleAmmo != null) {
            isAmmoBar = true;
        }
        ammoSlider.gameObject.SetActive(isAmmoBar);

        if (moduleHealth.GetComponent<TrainBuilding>()) {
            GetComponent<UIElementFollowWorldTarget>().SetUp(moduleHealth.GetComponent<TrainBuilding>().uiTargetTransform);
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
        SetHealthBarValue();
        if(isAmmoBar)
            SetAmmoBarValue();
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
            if (myModuleAmmo.ShowAmmoBar()) {
                var percent = (float)myModuleAmmo.curAmmo / myModuleAmmo.maxAmmo;
                percent = Mathf.Clamp(percent, 0, 1f);

                
                ammoSlider.value = percent;

                if(!ammoSlider.gameObject.activeSelf)
                    ammoSlider.gameObject.SetActive(true);
            } else {
                if(ammoSlider.gameObject.activeSelf)
                    ammoSlider.gameObject.SetActive(false);
            }
        }
    }
}
