 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoPauseSoundOnGamePause : MonoBehaviour
{
    private void OnEnable() {
        TimeController.PausedEvent.AddListener(OnPause);
    }

    private void OnDisable() {
        TimeController.PausedEvent.RemoveListener(OnPause);
    }


    void OnPause(bool isPaused) {
        if (isPaused) {
            GetComponent<AudioSource>().Pause();
        } else {
            GetComponent<AudioSource>().UnPause();
        }
    }
}
