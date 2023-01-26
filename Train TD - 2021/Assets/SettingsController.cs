using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour {
    public static SettingsController s;

    private void Awake() {
        s = this;
    }

    public GameObject settingsParent;
    
    void Start()
    {
        var initRequiredSettings = settingsParent.GetComponentsInChildren<IInitRequired>();
        for (int i = 0; i < initRequiredSettings.Length; i++) {
            initRequiredSettings[i].Initialize();
        }
    }

    
    public void ResetRun() {
        if (SceneLoader.s.isLevelInProgress) {
            MissionWinFinisher.s.Cleanup();
        }

        DataSaver.s.GetCurrentSave().currentRun = null;
        DataSaver.s.GetCurrentSave().isInARun = false;
        //DataSaver.s.GetCurrentSave().currentRun.SetCharacter( DataHolder.s.GetCharacter("Le Cheater"));
        //MapController.s.GenerateStarMap();
        DataSaver.s.SaveActiveGame();
        
        MenuToggle.HideAllToggleMenus();
        
        if (FirstTimeTutorialController.s.tutorialEngaged) {
            FirstTimeTutorialController.s.SkipTutorial();
        } else {
            SceneLoader.s.BackToStarterMenu();
        }
    }
    
    public void ResetRunAndReplayTutorial() {
        PlayerPrefs.SetInt("finishedTutorial", 0);
        FirstTimeTutorialController.s.ReDoTutorial();
        //ResetRun();
    }

    public void ClearCurrentSaveAndPlayerPrefs() {
        PlayerPrefs.DeleteAll();
        DataSaver.s.ClearCurrentSave();
    }
    
}

public interface IInitRequired {
    public void Initialize();
}
