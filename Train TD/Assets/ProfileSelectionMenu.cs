using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfileSelectionMenu : MonoBehaviour {
    public static ProfileSelectionMenu s;

    private void Awake() {
        s = this;
    }

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


    private void Start() {
        if (LevelLoader.s.isProfileMenu) {
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
        } else {
            StartGame();
        }
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

    public MiniGUI_ProfileDisplay currentProfile;
    public void SaveChanged() {
        currentProfile.SetStats(DataSaver.s.GetCurrentSave());
    }



    public void QuitGame() {
        Application.Quit();
    }
}