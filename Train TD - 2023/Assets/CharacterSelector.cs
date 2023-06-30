using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CharacterSelector : MonoBehaviour {
    public static CharacterSelector s;

    private void Awake() {
        s = this;
    }

    public GameObject charSelectUI;
    public Transform charsParent;
    public GameObject charPanelPrefab;
    

    public void CheckAndShowCharSelectionScreen() {
        if (!DataSaver.s.GetCurrentSave().isInARun) {
            charSelectUI.SetActive(true);
            PlayerWorldInteractionController.s.canSelect = false;
            SetUpCharPanel();
        } else {
            charSelectUI.SetActive(false);
            if (!MissionWinFinisher.s.isWon) {
                PlayerWorldInteractionController.s.canSelect = true;
            }
        }
    }

    void SetUpCharPanel() {
        charsParent.DeleteAllChildren();
        var allChars = DataHolder.s.characters;
        for (int i = 0; i < allChars.Length; i++) {
            var panel = Instantiate(charPanelPrefab, charsParent).GetComponent<MiniGUI_CharSelectPanel>();
            panel.Setup(allChars[i].myCharacter, !XPProgressionController.s.IsCharacterUnlocked(i), i==0);
        }
    }

    public void SelectCharacter(CharacterData _data) {
        DataSaver.s.GetCurrentSave().currentRun = new DataSaver.RunState();
        DataSaver.s.GetCurrentSave().currentRun.currentAct = 1;
        DataSaver.s.GetCurrentSave().currentRun.SetCharacter(_data);
        DataSaver.s.GetCurrentSave().isInARun = true;
        DataSaver.s.SaveActiveGame();

        PlayStateMaster.s.FinishCharacterSelection();
    }

    public void CharSelectionAndWorldGenerationComplete() {
        DataSaver.s.GetCurrentSave().isInARun = true;
    }
}
