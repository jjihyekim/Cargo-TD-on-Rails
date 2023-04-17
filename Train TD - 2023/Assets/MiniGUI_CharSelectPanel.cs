using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_CharSelectPanel : MonoBehaviour {
	public CharacterData myData;

	public TMP_Text charNameText;
	public TMP_Text charDescText;

	public GameObject lockedOverlay;

	public void Setup(CharacterData data, bool isLocked) {
		myData = data;
		charNameText.text = myData.uniqueName;
		charDescText.text = myData.description;
		
		lockedOverlay.SetActive(isLocked);
	}


	public void Select() {
		CharacterSelector.s.SelectCharacter(myData);
	}
}
