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

    public GameObject bossDetected;
    public GameObject shipDetected;
    public GameObject encounterDetected;
    
    private StarState currentStar;

    public bool isTravel;

    public GenericCallback hideCallback;
    public void Initialize(StarState star, LevelData connectionLevel, GenericCallback _hideCallback = null) {
        StopAllCoroutines();
        currentStar = star;
        starName.text = currentStar.starName;

        bossDetected.SetActive(star.isBoss);

        if (!star.isBoss && connectionLevel != null) {
            var isEncounter = connectionLevel.isEncounter;
            encounterDetected.SetActive(isEncounter);
            shipDetected.SetActive(!isEncounter);
        } else {
            encounterDetected.SetActive(false);
            shipDetected.SetActive(false);
        }

        gameObject.SetActive(true);

        hideCallback = _hideCallback;
    }
    
    public void Initialize(StarState sourceStar, StarState targetStar, LevelData connectionLevel) {
        Initialize(targetStar, connectionLevel);
        starName.text = $"{sourceStar.starName}\nto\n{targetStar.starName}";
    }

    public void SetSelectable(bool isSelectable) {
        var buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++) {
            buttons[i].interactable = isSelectable;
        }
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
        if (!isTravel) {
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                if (!RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), mousePos, OverlayCamsReference.s.uiCam)) {
                    Hide();
                }
            }
        }
    }

    public void Travel() {
        if (isTravel) {
            MapController.s.StartTravelingToStar();
        } else {
            MapController.s.SelectStar();
        }
    }

    public void CancelTravel() {
        Hide();
        SetUpBuyCargo.s.ClearPreviousCargo();
    }

    public void Hide() {
        gameObject.SetActive(false);
        hideCallback?.Invoke();
    }
}
