using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActFinishController : MonoBehaviour {
    public static ActFinishController s;

    private void Awake() {
        s = this;
    }

    private void Start() {
        act1WinUI.SetActive(false);
        act2WinUI.SetActive(false);
        act3WinUI.SetActive(false);
    }

    public GameObject act1WinUI;
    public GameObject act2WinUI;
    public GameObject act3WinUI;
    public void OpenActWinUI() {
        if (DataSaver.s.GetCurrentSave().currentRun.currentAct == 1) {
            act1WinUI.SetActive(true);
        }else if (DataSaver.s.GetCurrentSave().currentRun.currentAct == 2) {
            act2WinUI.SetActive(true);
        } else {
            act3WinUI.SetActive(true);
        }
    }


    public void StartNewAct() {
        if (DataSaver.s.GetCurrentSave().currentRun.currentAct == 3) {
            DataSaver.s.GetCurrentSave().currentRun = null;
            DataSaver.s.GetCurrentSave().isInARun = false;
            DataSaver.s.SaveActiveGame();
            StarterUIController.s.BackToProfileSelection();
            return;
        }
        
        DataSaver.s.GetCurrentSave().currentRun.currentAct += 1;
        MapController.s.GenerateStarMap();
        DataSaver.s.SaveActiveGame();
        MusicPlayer.s.SwapMusicTracksAndPlay(false);
        SceneLoader.s.BackToStarterMenuHardLoad();
    }
}
