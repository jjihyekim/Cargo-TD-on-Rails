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

    public Transform sourceTransform;

    public void SetUp(Cart building) {
        Show();

        var gunModule = building.GetComponentInChildren<GunModule>();
        if (gunModule != null) {
            armorPenetrationIcon.gameObject.SetActive(gunModule.canPenetrateArmor);
        } else {
            armorPenetrationIcon.gameObject.SetActive(false);
        }

        icon.sprite = building.Icon;
        moduleName.text = building.displayName;

        moduleDescription.text = building.GetComponentInChildren<ClickableEntityInfo>().GetTooltip().text;

        sourceTransform = building.GetUITargetTransform();

        if (building.uniqueName == "scrapPile" || building.uniqueName == "fuelPile") {
            infoCardsParent.gameObject.SetActive(false);
        } else {
            infoCardsParent.gameObject.SetActive(true);
            var infoCards = infoCardsParent.GetComponentsInChildren<IBuildingInfoCard>(true);
            for (int i = 0; i < infoCards.Length; i++) {
                infoCards[i].SetUp(building);
            }
        }
        GetComponentInParent<UIElementFollowWorldTarget>().SetUp(sourceTransform);
    }

    public void SetUp(PowerUpScriptable powerUp) {
        Show();
        armorPenetrationIcon.gameObject.SetActive(false);

        icon.sprite = powerUp.icon;
        moduleName.text = powerUp.name;
        
        //sourceTransform = powerUp.GetUITargetTransform();

        moduleDescription.text = powerUp.description;
        
        infoCardsParent.gameObject.SetActive(false);
        GetComponentInParent<UIElementFollowWorldTarget>().SetUp(sourceTransform);
    }
    
    public void SetUp(EnemyHealth enemy) {
        Show();
        armorPenetrationIcon.gameObject.SetActive(false);

        icon.sprite = enemy.GetComponentInParent<EnemyWave>().GetIcon();
        moduleName.text = enemy.GetComponentInParent<EnemyWave>().myEnemy.enemyUniqueName;
        var myInfo = enemy.GetComponentsInChildren<IClickableInfo>();
        moduleDescription.text =myInfo[0].GetInfo();
        
        
        infoCardsParent.gameObject.SetActive(false);
        
        
        sourceTransform = enemy.uiTransform;
        GetComponentInParent<UIElementFollowWorldTarget>().SetUp(sourceTransform);
    }

    public void Show() {
        transform.parent.gameObject.SetActive(true);
    }

    public void Hide() {
        transform.parent.gameObject.SetActive(false);
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
    
    /*private void Update() {
        if (sourceTransform == null) {
            PlayerModuleSelector.s.HideModuleActionSelector();
            return;
        }
    }*/
}

public interface IBuildingInfoCard {
    public void SetUp(Cart building);
}