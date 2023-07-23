using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingsController : MonoBehaviour {
    public static SettingsController s;

    private void Awake() {
        s = this;
        settingsParent.SetActive(false);
    }

    public GameObject settingsParent;

    public bool forceDisableGamepadMode = false;
    public bool forceEnableGamepadMode = false;
    
    void Start()
    {
#if !UNITY_EDITOR
        forceDisableGamepadMode = false;
        forceEnableGamepadMode = false;
#endif
        
        var initRequiredSettings = settingsParent.GetComponentsInChildren<IInitRequired>();
        for (int i = 0; i < initRequiredSettings.Length; i++) {
            initRequiredSettings[i].Initialize();
        }
    }


    
    public void ResetRun() {
        /*if (PlayStateMaster.s.isCombatStarted()) {
            MissionWinFinisher.s.ContinueToClearOutOfCombat();
        }*/

        DataSaver.s.GetCurrentSave().currentRun = null;
        DataSaver.s.GetCurrentSave().isInARun = false;
        
        DataSaver.s.SaveActiveGame();
        
        SceneLoader.s.ForceReloadScene();

        /*MenuToggle.HideAllToggleMenus();
        
        if (FirstTimeTutorialController.s.tutorialEngaged) {
            FirstTimeTutorialController.s.SkipTutorial();
        } else {
            PlayStateMaster.s.EnterShopState();
        }
        
        Pauser.s.Unpause();*/

        //SFX
        AudioManager.PlayOneShot(SfxTypes.ButtonClick1);
    }
    
    public void ResetRunAndReplayTutorial() {
        MenuToggle.HideAllToggleMenus();
        Pauser.s.Unpause();
        
        FirstTimeTutorialController.s.ReDoTutorial();
        //ResetRun();

        //SFX
        AudioManager.PlayOneShot(SfxTypes.ButtonClick1);
    }

    public void ClearCurrentSaveAndPlayerPrefs() {
        PlayerPrefs.DeleteAll();
        DataSaver.s.ClearCurrentSave();
    }

    public void ReloadScene() {
        SceneLoader.s.ForceReloadScene();

        //SFX
        AudioManager.PlayOneShot(SfxTypes.ButtonClick1);
    }


    public static bool GamepadMode() {
        if (s != null)
            return (Gamepad.all.Count > 0 && !s.forceDisableGamepadMode) || s.forceEnableGamepadMode;
        else
            return Gamepad.all.Count > 0;
    }

    public static bool ShowButtonPrompts() {
        return MiniGUI_ShowButtonHints.ShowButtonHints();
    }

    public void StartAutoRunFast() {
        AutoPlaytester.s.StartAutoPlayer(true);
    }

    public void StartAutoRun() {
        AutoPlaytester.s.StartAutoPlayer(false);
    }
}

public interface IInitRequired {
    public void Initialize();
}
