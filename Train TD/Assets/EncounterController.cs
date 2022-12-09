using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncounterController : MonoBehaviour {
    public static EncounterController s;

    public GameObject[] stuffToDisableDuringEncounter;
    public CameraController camCont;

    public GameObject encounterTextUI;
    public TMP_Text titleText;
    public TMP_Text mainText;

    public Transform optionsParent;
    public GameObject optionsPrefab;

    public RandomEncounter currentEncounter;

    
    private void Awake() {
        s = this;
    }

    private void Start() {
        encounterTextUI.SetActive(false);
        fadeUI.SetActive(false);
    }

    public void EngageEncounter(LevelData data) {
        currentEncounter = Instantiate(DataHolder.s.GetEncounter(data.levelName), Vector3.zero, Quaternion.identity).GetComponent<RandomEncounter>();
        StartCoroutine(EncounterAnimation());
    }

    public GameObject fadeUI;
    public Image fadeOverlay;
    public float fadeDurations = 0.5f;

    IEnumerator EncounterAnimation() {
        fadeUI.SetActive(true);
        StartCoroutine(FadeOverlay(0, 1, fadeDurations));
        
        yield return new WaitForSeconds(fadeDurations);
        
        for (int i = 0; i < stuffToDisableDuringEncounter.Length; i++) {
            if(stuffToDisableDuringEncounter[i] != null)
                stuffToDisableDuringEncounter[i].SetActive(false);
        }
        
        StartCoroutine(FadeOverlay(1, 0, fadeDurations));
        
        var train = Train.s.gameObject;
        camCont.enabled = false;

        var encounterCamPos = currentEncounter.transform.GetChild(0);
        MainCameraReference.s.cam.transform.position = encounterCamPos.position;
        MainCameraReference.s.cam.transform.rotation = encounterCamPos.rotation;

        var pos = train.transform.position;
        
        // s = (v+u)t/2
        // v = 2s/t
        float dist = 15;
        float timer = 3f;
        
        float v = 2*dist/timer;
        float a = v/timer;


        pos.z = -dist + (train.transform.position - Train.s.trainFront.position).z;
        
        while (timer > 0f) {
            pos.z += v * Time.deltaTime;
            v += -a * Time.deltaTime;
            train.transform.position = pos;

            timer -= Time.deltaTime;
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(FadeOverlay(0, 1, fadeDurations));
        yield return new WaitForSeconds(fadeDurations);
        
        train.transform.position = Vector3.zero;
        camCont.enabled = true;
        
        DisplayEncounterOptions();
        
        
        StartCoroutine(FadeOverlay(1, 0, fadeDurations));
        fadeUI.SetActive(false);
    }
    
    IEnumerator FadeOverlay(float startValue, float targetValue, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            var col = fadeOverlay.color;
            col.a = Mathf.Lerp(startValue, targetValue, time / duration);
            fadeOverlay.color = col;
            
            time += Time.deltaTime;
            yield return null;
        }
        var col2 = fadeOverlay.color;
        col2.a = targetValue;
        fadeOverlay.color = col2;
    }

    void DisplayEncounterOptions() {
        titleText.text = currentEncounter.encounterTitleName;
        mainText.text = currentEncounter.richEncounterText;
        
        encounterTextUI.SetActive(true);

        
        optionsParent.DeleteAllChildren();
        
        var optionAvailability = currentEncounter.SetOptionTexts();
        for (int i = 0; i < currentEncounter.options.Length; i++) {
            var option = Instantiate(optionsPrefab, optionsParent);
            option.GetComponentInChildren<TMP_Text>().text = currentEncounter.options[i];

            var button = option.GetComponent<Button>();

            var index = i;
            button.onClick.AddListener(() => SelectOption(index));
            
            if (optionAvailability != null && i < optionAvailability.Length) {
                button.interactable = optionAvailability[i];
            }
        }
    }


    void SelectOption(int i) {
        var newEncounter = currentEncounter.OptionPicked(i);
        
        if (newEncounter != null) {
            newEncounter.encounterTitleName = currentEncounter.encounterTitleName;
            currentEncounter = newEncounter;
            DisplayEncounterOptions();
        } else {
            EncounterComplete();
        }
    }

    void EncounterComplete() {
        DataSaver.s.GetCurrentSave().currentRun.unclaimedRewards = new List<string>();
        DataSaver.s.GetCurrentSave().currentRun.shopInitialized = false;
        MapController.s.FinishTravelingToStar();
        DataSaver.s.SaveActiveGame();
        SceneLoader.s.BackToStarterMenuHardLoad();
    }
}
