using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfOpenedGuideBefore : MonoBehaviour {

    public static bool shouldDisableBecauseOfTutorial = false;

    public void OpenGuide() {
        PlayerPrefs.SetInt("openedGuide", 1);
        DisableThisThing();
    }
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(LateStart),0.01f);
    }

    void LateStart() {
        if (shouldDisableBecauseOfTutorial) {
            DisableThisThing();
        } else {
            if (PlayerPrefs.GetInt("openedGuide", 0) == 0) {
                // do nothing
            } else {
                DisableThisThing();
            }
        }
    }

    void DisableThisThing() {
        gameObject.SetActive(false);
    }
}
