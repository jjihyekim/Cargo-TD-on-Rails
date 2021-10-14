using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGUI_MouseMissionDistanceDisplay : MonoBehaviour {
    public Vector2 offset = new Vector2(10, -10);


    private TMP_Text myText;
    private void Start() {
        myText = GetComponent<TMP_Text>();
    }

    private void Update() {
        var mousePos = Mouse.current.position.ReadUnprocessedValue();
        transform.position = mousePos + offset;
        myText.text = LevelEditorController.s.ScreenPositionToLevelPosition(mousePos.x).ToString("F2");
    }
}
