using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadControlsHelper : MonoBehaviour {
    public static GamepadControlsHelper s;

    private void Awake() {
        s = this;
    }

    public GameObject gamepadSelector;

    public float rotateSpeed = 20f;
    public float regularSize = 1f;
    public float clickSize = 0.7f;
    public float sizeLerpSpeed = 1f;

    public InputActionReference clickAction;

    // Update is called once per frame
    void Update()
    {
        if (SettingsController.GamepadMode() && PlayerWorldInteractionController.s.canSelect) {
            gamepadSelector.SetActive(true);


            if (clickAction.action.IsPressed()) {
                gamepadSelector.transform.localScale = Vector3.Lerp(gamepadSelector.transform.localScale, Vector3.one * clickSize, sizeLerpSpeed * Time.deltaTime);

            } else {
                gamepadSelector.transform.localScale = Vector3.Lerp(gamepadSelector.transform.localScale, Vector3.one * regularSize, sizeLerpSpeed * Time.deltaTime);
                gamepadSelector.transform.Rotate(0,rotateSpeed*Time.deltaTime,0);
                
            }
            
            
        } else {
            gamepadSelector.SetActive(false);
        }
    }

    public Ray GetRay() {
        return new Ray(gamepadSelector.transform.position + Vector3.up * 3, Vector3.down);
    }

    public Vector3 GetTooltipPosition() {
        return gamepadSelector.transform.position + Vector3.up * 0.5f;
    }
}
