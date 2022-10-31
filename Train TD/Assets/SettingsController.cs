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
        DataSaver.s.GetCurrentSave().currentRun = null;
        DataSaver.s.GetCurrentSave().isInARun = false;
        //DataSaver.s.GetCurrentSave().currentRun.SetCharacter( DataHolder.s.GetCharacter("Le Cheater"));
        //MapController.s.GenerateStarMap();
        DataSaver.s.SaveActiveGame();
        SceneLoader.s.BackToStarterMenuHardLoad();
    }
    
}

public interface IInitRequired {
    public void Initialize();
}
