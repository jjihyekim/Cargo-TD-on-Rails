using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeController : MonoBehaviour {
    public static TimeController s;

    private void Awake() {
        s = this;
    }


    public float currentTimeScale = 1f;

    private bool isPaused = false;
    public void Pause() {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void UnPause() {
        isPaused = false;
        Time.timeScale = currentTimeScale;
    }


    public InputActionReference fastForwardKey;

    private void OnEnable() {
        fastForwardKey.action.Enable();
    }

    private void OnDisable() {
        fastForwardKey.action.Disable();
    }

    private void Update() {
        ProcessFastForward();
    }

    public void ProcessFastForward() {
        //print(fastForwardKey.action.ReadValue<float>());
        if (fastForwardKey.action.ReadValue<float>() > 0f) {
            currentTimeScale = 8f;
            if (!isPaused) {
                Time.timeScale = currentTimeScale;
            }
        } else {
            currentTimeScale = 1f;
            if (!isPaused) {
                Time.timeScale = currentTimeScale;
            }
        }
    }
}
