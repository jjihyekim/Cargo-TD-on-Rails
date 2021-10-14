using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MiniGUI_LevelEditorLane : MonoBehaviour {

	public bool isLeft;
	public EnemyWaveData.EnemyPathType myPathType;

	public RectTransform enemiesParent;
	
	public void OnPointerClick(PointerEventData eventData) {
		LevelEditorController.s.OnPointerClick(eventData,this);
	}

	public void OnBeginDrag(PointerEventData eventData) {
		LevelEditorController.s.OnBeginDrag(eventData, this);
	}

	public void OnDrag(PointerEventData eventData) {
		LevelEditorController.s.OnDrag(eventData, this);
	}

	public void OnEndDrag(PointerEventData eventData) {
		LevelEditorController.s.OnEndDrag(eventData, this);
	}

}
