using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfOpenedGuideBefore : MonoBehaviour {

    public void OpenGuide() {
        PlayerPrefs.SetInt("openedGuide", 1);
        DisableThisThing();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("openedGuide", 0) == 0) {
            // do nothing
        } else {
            DisableThisThing();
        }
    }

    void DisableThisThing() {
        gameObject.SetActive(false);
    }
}
