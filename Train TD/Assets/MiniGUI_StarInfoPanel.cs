using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MiniGUI_StarInfoPanel : MonoBehaviour {

    public GameObject infoScreen;
    public TMP_Text starName;

    public GameObject miniPortal;
    public TMP_Text miniPortalText;
    public GameObject shipDetected;
    public TMP_Text shipDetectedText;
    
    private StarState currentStar;
    public void Initialize(StarState star) {
        StopAllCoroutines();
        currentStar = star;
        starName.text = currentStar.starName;

        miniPortal.SetActive(currentStar.isShop || currentStar.isBoss);
        
        if (currentStar.isBoss) {
            miniPortalText.text = "Boss";
        } else {
            miniPortalText.text = "Shop";
        }

        var shipSizeText = "Unknown Enemies";
        /*if (currentStar.enemyShip.HullModule.DisplayName.Contains("SMALL")) {
            shipSizeText = "Small Ship";
        }else if (currentStar.enemyShip.HullModule.DisplayName.Contains("MEDIUM")) {
            shipSizeText = "Medium Ship";
        }else if (currentStar.enemyShip.HullModule.DisplayName.Contains("BIG")) {
            shipSizeText = "Big Ship";
        }*/

        shipDetectedText.text = shipSizeText;
        
        
        gameObject.SetActive(true);
    }

    /*IEnumerator Scan() {
        var timer = 0f;

        while (timer < scanTime) {
            scanSlider.value = timer / scanTime;
            timer += Time.deltaTime;
            yield return null;
        }

        currentStar.isStarInfoEverShown = true;
        scanScreen.SetActive(false);
        infoScreen.SetActive(true);
    }*/

    
    private void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (!RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), mousePos, Camera.main)) {
                Hide();
            }
        }
    }

    public void Travel() {
        MapController.s.TravelToStar();
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
