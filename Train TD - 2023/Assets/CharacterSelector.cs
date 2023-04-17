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


    public UnityEvent OnCharacterSelected = new UnityEvent();

    public void CheckAndShowCharSelectionScreen() {
        if (!DataSaver.s.GetCurrentSave().isInARun) {
            charSelectUI.SetActive(true);
            SetUpCharPanel();
        } else {
            charSelectUI.SetActive(false);
        }
    }

    void SetUpCharPanel() {
        charsParent.DeleteAllChildren();
        var allChars = DataHolder.s.characters;
        for (int i = 0; i < allChars.Length; i++) {
            var panel = Instantiate(charPanelPrefab, charsParent).GetComponent<MiniGUI_CharSelectPanel>();
            panel.Setup(allChars[i].myCharacter, !XPProgressionController.s.IsCharacterUnlocked(i));
        }
    }

    private CharacterData data;
    private bool characterChangeInProgress = false;

    public void SelectCharacter(CharacterData _data) {
        if (!characterChangeInProgress) {
            if (SceneLoader.s.isProfileMenu()) {
                ProfileSelectionMenu.s.StartGame();
            } else {
                SceneLoader.s.BackToStarterMenu(true);
            }
            
            characterChangeInProgress = true;
            data = _data;
            SceneLoader.s.afterTransferCalls.Enqueue(() => DoTransfer());
        }
    }

    public void SelectCharacterInstant(CharacterData _data) {
        data = _data;
        DataSaver.s.GetCurrentSave().currentRun = new DataSaver.RunState();
        DataSaver.s.GetCurrentSave().currentRun.currentAct = 1;
        DataSaver.s.GetCurrentSave().currentRun.SetCharacter(data);
        DataSaver.s.GetCurrentSave().isInARun = true;
        MapController.s.GenerateStarMap();
        DataSaver.s.SaveActiveGame();
    }

    void DoTransfer() {
        DataSaver.s.GetCurrentSave().currentRun = new DataSaver.RunState();
        DataSaver.s.GetCurrentSave().currentRun.currentAct = 1;
        DataSaver.s.GetCurrentSave().currentRun.SetCharacter(data);
        DataSaver.s.GetCurrentSave().isInARun = true;
        Train.s.DrawTrainBasedOnSaveData();
        MapController.s.GenerateStarMap();
        WorldMapCreator.s.GenerateWorldMap();
        HexGrid.s.RefreshGrid();
        UpgradesController.s.callWhenUpgradesOrModuleAmountsChanged?.Invoke();
        Pauser.s.Unpause();
        PlayerActionsController.s.UpdatePowerUpButtons();
        UpgradesController.s.DrawShopOptions();

        DataSaver.s.SaveActiveGame();
        MusicPlayer.s.SwapMusicTracksAndPlay(false);
        characterChangeInProgress = false;
        
        OnCharacterSelected?.Invoke();
    }
}
