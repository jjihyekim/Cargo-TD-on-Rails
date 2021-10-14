using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MiniGUI_LevelEditorLaneInputHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {
	
	
	
	public void OnPointerClick(PointerEventData eventData) {
		GetComponentInParent<MiniGUI_LevelEditorLane>().OnPointerClick(eventData);
	}

	public void OnBeginDrag(PointerEventData eventData) {
		GetComponentInParent<MiniGUI_LevelEditorLane>().OnBeginDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData) {
		GetComponentInParent<MiniGUI_LevelEditorLane>().OnDrag(eventData);
	}

	public void OnEndDrag(PointerEventData eventData) {
		GetComponentInParent<MiniGUI_LevelEditorLane>().OnEndDrag(eventData);
	}
}
