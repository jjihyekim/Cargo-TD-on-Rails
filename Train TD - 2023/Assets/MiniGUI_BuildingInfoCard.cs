using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniGUI_BuildingInfoCard : MonoBehaviour
{
    public Image icon;
    public Image armorPenetrationIcon;
    public TMP_Text moduleName;
    [Space] 
    public TMP_Text moduleDescription;
    
    public Transform infoCardsParent;


    public Button GetBackToMainAfterCargoButton;

    private TrainBuilding cargoBuilding;

    public Transform sourceTransform;

    public void SetUp(TrainBuilding building, bool overrideCargo = true) {
        GetBackToMainAfterCargoButton.gameObject.SetActive(false);
        if (overrideCargo)
            cargoBuilding = building;

        var gunModule = building.GetComponent<GunModule>();
        if (gunModule != null) {
            armorPenetrationIcon.gameObject.SetActive(gunModule.canPenetrateArmor);
        } else {
            armorPenetrationIcon.gameObject.SetActive(false);
        }

        icon.sprite = building.Icon;
        moduleName.text = building.displayName;

        moduleDescription.text = building.GetComponent<ClickableEntityInfo>().GetTooltip().text;


        if (building.uniqueName == "scrapPile" || building.uniqueName == "fuelPile") {
            infoCardsParent.gameObject.SetActive(false);
        } else {
            infoCardsParent.gameObject.SetActive(true);
            var infoCards = infoCardsParent.GetComponentsInChildren<IBuildingInfoCard>(true);
            for (int i = 0; i < infoCards.Length; i++) {
                infoCards[i].SetUp(building);
            }
        }

        if (overrideCargo) {
            sourceTransform = building.GetUITargetTransform(false);
            GetComponentInParent<UIElementFollowWorldTarget>().SetUp(sourceTransform);
        }
    }

    public void SetUp(PowerUpScriptable powerUp) {
        GetBackToMainAfterCargoButton.gameObject.SetActive(false);
        
        armorPenetrationIcon.gameObject.SetActive(false);

        icon.sprite = powerUp.icon;
        moduleName.text = powerUp.name;

        moduleDescription.text = powerUp.description;
        
        infoCardsParent.gameObject.SetActive(false);
    }
    
    public void SetUp(EnemyHealth enemy) {
        GetBackToMainAfterCargoButton.gameObject.SetActive(false);
        
        armorPenetrationIcon.gameObject.SetActive(false);

        icon.sprite = enemy.GetComponentInParent<EnemyWave>().GetIcon();
        moduleName.text = enemy.GetComponentInParent<EnemyWave>().myEnemy.enemyUniqueName;
        var myInfo = enemy.GetComponentsInChildren<IClickableInfo>();
        moduleDescription.text =myInfo[0].GetInfo();
        
        
        infoCardsParent.gameObject.SetActive(false);
        
        
        sourceTransform = enemy.uiTransform;
        GetComponentInParent<UIElementFollowWorldTarget>().SetUp(sourceTransform);
    }


    public void ShowInfoAboutCargo(TrainBuilding building) {
        SetUp(building, false);
        GetBackToMainAfterCargoButton.gameObject.SetActive(true);
    }
    
    public void ShowInfoAboutCargo(PowerUpScriptable powerUp) {
        SetUp(powerUp);
        GetBackToMainAfterCargoButton.gameObject.SetActive(true);
    }

    public void GetBackToMainAfterCargo() {
        SetUp(cargoBuilding);
    }
    
    public RectTransform reticle;
    public List<RectTransform> extraRects = new List<RectTransform>();

    public bool IsMouseOverMenu() {
        if (!gameObject.activeSelf)
            return false;
	    
        Vector2 mousePos = Mouse.current.position.ReadValue();
        var rect = reticle;
        var isOverRect = RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, OverlayCamsReference.s.uiCam);

        for (int i = 0; i < extraRects.Count; i++) {
            if (extraRects[i] != null) {
                var isOverButton = RectTransformUtility.RectangleContainsScreenPoint(extraRects[i], mousePos, OverlayCamsReference.s.uiCam);
                isOverRect = isOverRect || isOverButton;
            }
        }

        return isOverRect;
    }
    
    private void Update() {
        if (sourceTransform == null) {
            PlayerModuleSelector.s.HideModuleActionSelector();
            return;
        }
    }
}

public interface IBuildingInfoCard {
    public void SetUp(TrainBuilding building);
}