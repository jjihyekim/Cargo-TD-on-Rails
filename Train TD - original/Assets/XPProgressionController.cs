using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XPProgressionController : MonoBehaviour {

	public static XPProgressionController s;

	private void Awake() {
		s = this;
	}

	 DataSaver.XPProgress _xpProgress => DataSaver.s.GetCurrentSave().xpProgress;

	public  bool IsCharacterUnlocked(int id) {
		switch (id) {
			case 0:
				return true;
			case 1:
				if (_xpProgress.xp > 5) {
					return true;
				} else {
					return false;
				}
			case 2:
				if (_xpProgress.xp > 10) {
					return true;
				} else {
					return false;
				}
				break;
			default:
				return false;
		}
	}
}
