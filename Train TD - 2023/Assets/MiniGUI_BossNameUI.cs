using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MiniGUI_BossNameUI : MonoBehaviour {

	public static MiniGUI_BossNameUI s;

	private void Awake() {
		s = this;
	}

	public TMP_Text myText;
	private Animation myAnim;
	private void Start() {
		myAnim = GetComponent<Animation>();
	}


	[Button]
	public void ShowBossName(string bossName) {
		myText.text = bossName;
		myAnim.Play();
	}
}
