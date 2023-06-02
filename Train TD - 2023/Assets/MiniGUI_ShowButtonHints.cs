using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_ShowButtonHints : MonoBehaviour, IInitRequired
{
    public const string exposedName = "buttonHints";
    public Toggle myToggle;
    
    public void Initialize() {
        var toggleVal = PlayerPrefs.GetInt(exposedName, 1);
        var val = toggleVal == 1;
        myToggle.isOn = val;
        SetVal(val);
    }

    public void OnToggleUpdated() {
        PlayerPrefs.SetInt(exposedName, myToggle.isOn ? 1 : 0);
        SetVal(myToggle.isOn);
    }

    void SetVal(bool val) {
        DataSaver.s.GetCurrentSave().tutorialProgress.firstCityTutorialDone = !val;
    }

    public static bool ShowButtonHints() {
        return PlayerPrefs.GetInt(exposedName, 1) == 1;
    }
}