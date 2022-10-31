using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelector : MonoBehaviour {
    public static CharacterSelector s;

    private void Awake() {
        s = this;
    }

    public GameObject charSelectUI;
    public Transform charsParent;
    public GameObject charPanelPrefab;
    void Start()
    {
        if (!DataSaver.s.GetCurrentSave().isInARun) {
            charSelectUI.SetActive(true);
            SetUpCharPanel();
        } else {
            charSelectUI.SetActive(false);
        }
    }

    void SetUpCharPanel() {
        var allChars = DataHolder.s.characters;
        for (int i = 0; i < allChars.Length; i++) {
            var panel = Instantiate(charPanelPrefab, charsParent).GetComponent<MiniGUI_CharSelectPanel>();
            panel.Setup(allChars[i].myCharacter);
        }
    }

    public void SelectCharacter(CharacterData data) {
        DataSaver.s.GetCurrentSave().currentRun = new DataSaver.RunState();
        DataSaver.s.GetCurrentSave().currentRun.SetCharacter(data);
        DataSaver.s.GetCurrentSave().isInARun = true;
        MapController.s.GenerateStarMap();
        DataSaver.s.SaveActiveGame();
        SceneLoader.s.BackToStarterMenuHardLoad();
    }
}
