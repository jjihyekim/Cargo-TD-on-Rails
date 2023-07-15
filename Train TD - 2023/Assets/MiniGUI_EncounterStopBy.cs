using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_EncounterStopBy : MonoBehaviour {

    public TMP_Text titleText;
    public Image myImage;
    public Slider timeoutSlider;

    public float timeoutTimer = 0;
    public float totalTime;

    public bool optionChoosen = true;
    
    public Color regularColor = Color.white;
    public Color flashColor = Color.blue;

    public Button stopButton;
    public Button ridePastButton;

    public void SetUp(EncounterTitle myEncounter) {
        timeoutTimer = myEncounter.autoRidePastTimer;
        totalTime = timeoutTimer;
        titleText.text = myEncounter.doYouWantToStopText;
        
        stopButton.interactable = true;
        ridePastButton.interactable = true;
        optionChoosen = false;
        myImage.sprite = myEncounter.image;
    }

    public void Stop() {
        if (!optionChoosen) {
            ChooseOption();
            StartCoroutine(FlashButton(stopButton, () => EncounterController.s.Stop()));
        }
    }

    public void RidePast() {
        if (!optionChoosen) {
            ChooseOption();
            StartCoroutine(FlashButton(ridePastButton, () => EncounterController.s.RidePastBeforeEncounter()));
        }
    }

    private void ChooseOption() {
        optionChoosen = true;

        stopButton.interactable = false;
        ridePastButton.interactable = false;

        PlayerWorldInteractionController.s.canSelect = false;
    }

    IEnumerator FlashButton(Button button, Action toCall) {
        for (int i = 0; i < 3; i++) {
            button.GetComponent<Image>().color = flashColor;
            yield return new WaitForSeconds(0.2f);
            button.GetComponent<Image>().color = regularColor;
            yield return new WaitForSeconds(0.2f);
        }

        toCall();
    }

    private void Update() {
        if (!optionChoosen) {
            timeoutTimer -= Time.deltaTime;

            if (timeoutTimer <= 0) {
                RidePast();
            }

            timeoutSlider.value = Mathf.Clamp01(1- timeoutTimer / totalTime);
        }
    }
}
