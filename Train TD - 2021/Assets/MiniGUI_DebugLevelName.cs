using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_DebugLevelName : MonoBehaviour {
	public static MiniGUI_DebugLevelName s;

	private void Awake() {
		s = this;
	}

	private void Start() {
		SetLevelName("");
	}

	public void SetLevelName(string levelName) {
		if (levelName.Length <= 0)
			levelName = "Not Selected";
		
		GetComponent<TMP_Text>().text = $"Current Level: {levelName}";
	}
}
