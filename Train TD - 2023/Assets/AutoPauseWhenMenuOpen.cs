using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPauseWhenMenuOpen : MonoBehaviour {
    private void OnEnable() {
        if (SceneLoader.s.isLevelInProgress) {
            Pauser.s.Pause();
        }
    }

    private void OnDisable() {
        if (SceneLoader.s.isLevelInProgress) {
            Pauser.s.Unpause();
        }
    }
}
