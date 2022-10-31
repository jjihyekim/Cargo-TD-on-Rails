using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_CharSelectPanel : MonoBehaviour {
	public CharacterData myData;

	public TMP_Text charNameText;
	public TMP_Text charDescText;
	
	public void Setup(CharacterData data) {
		myData = data;
		charNameText.text = myData.uniqueName;
		charDescText.text = myData.description;
	}


	public void Select() {
		CharacterSelector.s.SelectCharacter(myData);
	}
}
