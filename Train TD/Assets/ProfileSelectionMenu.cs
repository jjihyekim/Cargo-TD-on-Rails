using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSelectionMenu : MonoBehaviour {
    public static ProfileSelectionMenu s;


    public GameObject[] goToDisableInProfileMenu;
    public MonoBehaviour[] monoToDisableInProfileMenu;
    public GameObject[] goToEnableInProfileMenu;
    public CameraSwitcher profileCameraSwithcer;


    [Space]
    
    public GameObject ProfileUI;
    public MonoBehaviour[] monoToEnableAfterProfileMenu;
    public MonoBehaviour[] monoToDisableAfterProfileMenu;
    public GameObject[] goToEnableAfterProfileMenu;
    public GameObject[] goToDisableAfterProfileMenu;

    public MiniGUI_ProfileDisplay currentProfile;


    public GameObject deleteSaveUI;

    private DataSaver.SaveFile saveToBeDeleted;
    public MiniGUI_ProfileDisplay deleteProfileDisplay;
    private void Awake() {
        s = this;
    }

    private void Start() {
        deleteSaveUI.SetActive(false);
        if (SceneLoader.s.isProfileMenu()) {
            OpenProfileMenu();
        } else {
            OpenProfileMenu(); // Because we need this to disable some stuff for us
            StartGame();
        }
    }

    private void OpenProfileMenu() {
        for (int i = 0; i < monoToDisableInProfileMenu.Length; i++) {
            monoToDisableInProfileMenu[i].enabled = false;
        }

        for (int i = 0; i < goToDisableInProfileMenu.Length; i++) {
            goToDisableInProfileMenu[i].SetActive(false);
        }

        for (int i = 0; i < goToEnableInProfileMenu.Length; i++) {
            goToEnableInProfileMenu[i].SetActive(true);
        }

        profileCameraSwithcer.Engage();

        ProfileUI.SetActive(true);
    }

    public void StartGame() {
        profileCameraSwithcer.Disengage();
        ProfileUI.SetActive(false);

        for (int i = 0; i < monoToEnableAfterProfileMenu.Length; i++) {
            monoToEnableAfterProfileMenu[i].enabled = true;
        }
        
        for (int i = 0; i < goToEnableAfterProfileMenu.Length; i++) {
            goToEnableAfterProfileMenu[i].SetActive(true);
        }
        
        for (int i = 0; i < goToDisableAfterProfileMenu.Length; i++) {
            goToDisableAfterProfileMenu[i].SetActive(false);
        }
        
        for (int i = 0; i < monoToDisableAfterProfileMenu.Length; i++) {
            monoToDisableAfterProfileMenu[i].enabled = false;
        }
    }

    public void SaveChanged() {
        currentProfile.SetStats(DataSaver.s.GetCurrentSave());
    }

    public void DeleteSaveDialog(DataSaver.SaveFile saveFile) {
        deleteSaveUI.SetActive(true);
        deleteProfileDisplay.SetStats(saveFile);
    }

    public void DeleteConfirm() {
        deleteSaveUI.SetActive(false);
        DataSaver.s.ClearCurrentSave();
        SaveChanged();
        ProfileUI.SetActive(false);
        ProfileUI.SetActive(true);
        SceneLoader.s.OpenProfileScreen();
    }

    public void DeleteCancel() {
        deleteSaveUI.SetActive(false);
    }



    public void QuitGame() {
        Application.Quit();
    }
}