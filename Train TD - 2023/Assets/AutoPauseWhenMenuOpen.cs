using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPauseWhenMenuOpen : MonoBehaviour {
    private void OnEnable() {
        if (PlayStateMaster.s.isCombatInProgress()) {
            Pauser.s.Pause();
        }
    }

    private void OnDisable() {
        if (PlayStateMaster.s.isCombatInProgress()) {
            Pauser.s.Unpause();
        }
    }
}
