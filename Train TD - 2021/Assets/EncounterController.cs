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
    public Image image;

    public Transform optionsParent;
    public GameObject optionsPrefab;
    public GameObject requirementOrRewardPrefab;

    public EncounterTitle currentEncounter;
    public EncounterNode currentNode;

    
    private void Awake() {
        s = this;
    }

    private void Start() {
        encounterTextUI.SetActive(false);
        fadeUI.SetActive(false);
    }

    
    public void EngageEncounter(LevelData data) {
        EngageEncounter(data.levelName);
    }

    public GameObject encounterObj;
    public void EngageEncounter(string encounterName) {
        encounterObj = Instantiate(DataHolder.s.GetEncounter(encounterName), Vector3.zero, Quaternion.identity);
        currentEncounter = encounterObj.GetComponent<EncounterTitle>();
        currentNode = currentEncounter.initialNode;
        StartCoroutine(EncounterStartAnimation());
    }

    public GameObject fadeUI;
    public Image fadeOverlay;
    public float fadeDurations = 0.5f;

    private float a;
    IEnumerator EncounterStartAnimation() {
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

        var encounterCamPos = currentEncounter.encounterCam;
        MainCameraReference.s.cam.transform.position = encounterCamPos.position;
        MainCameraReference.s.cam.transform.rotation = encounterCamPos.rotation;

        var pos = train.transform.position;
        
        // s = (v+u)t/2
        // v = 2s/t
        float dist = 15;
        float timer = 3f;
        
        float v = 2*dist/timer;
        a = v/timer;


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
        
        StartEncounter();
        
        
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

    void StartEncounter() {
        encounterTextUI.SetActive(true);

        image.sprite = currentEncounter.image;
        titleText.text = currentEncounter.title;
        
        foreach (var randomizer in currentEncounter.GetComponentsInChildren<EncounterRandomlyEnableOption>()) {
            randomizer.Randomize();
        }
        
        foreach (var reward in currentEncounter.GetComponentsInChildren<EncounterReward>()) {
            reward.RandomizeReward();
        }
        
        foreach (var requirement in currentEncounter.GetComponentsInChildren<EncounterRequirement>()) {
            requirement.RandomizeRequirement();
        }
        
        MoveToNewNode(currentEncounter.initialNode);
    }


    public void MoveToNewNode(EncounterNode node) {
        if (node == null) {
            encounterTextUI.SetActive(false);
            Invoke(nameof(EncounterComplete), fadeDurations);
            StartCoroutine(EncounterEndAnimation());
            return;
        }

        currentNode = node;
        mainText.text = currentNode.text;

        optionsParent.DeleteAllChildren();

        var currentOptions = currentNode.GetComponentsInChildren<EncounterOption>();
        for (int i = 0; i < currentOptions.Length; i++) {
            if (currentOptions[i].optionEnabled) {
                var option = Instantiate(optionsPrefab, optionsParent);
                option.GetComponent<MiniGUI_EncounterOption>().SetUp(currentOptions[i]);
            }
        }
    }

    void EncounterComplete() {
        DataSaver.s.GetCurrentSave().currentRun.unclaimedRewards = new List<string>();
        DataSaver.s.GetCurrentSave().currentRun.shopInitialized = false;
        MapController.s.FinishTravelingToStar();
        DataSaver.s.SaveActiveGame();
        SceneLoader.s.BackToStarterMenu();
        SceneLoader.s.afterTransferCalls.Enqueue(() => DoComplete());
    }
    
    
    IEnumerator EncounterEndAnimation() {
        var train = Train.s.gameObject;
        camCont.enabled = false;

        var pos = train.transform.position;
        
        
        float timer = fadeDurations + SceneLoader.s.fadeTime;
        
        float v = 0;

        while (timer > 0f) {
            pos.z += v * Time.deltaTime;
            v += a * Time.deltaTime;
            train.transform.position = pos;

            timer -= Time.deltaTime;
            yield return null;
        }

        train.transform.position = Vector3.zero;
        camCont.enabled = true;
    }

    void DoComplete() {
        if (encounterObj != null) {
            Destroy(encounterObj);
        }
        
        for (int i = 0; i < stuffToDisableDuringEncounter.Length; i++) {
            if(stuffToDisableDuringEncounter[i] != null)
                stuffToDisableDuringEncounter[i].SetActive(true);
        }
    }
}
