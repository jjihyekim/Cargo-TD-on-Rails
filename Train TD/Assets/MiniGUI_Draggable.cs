using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.InputSystem;

public class MiniGUI_Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	public Transform target;
	private bool isMouseDown = false;
	private Vector3 startMousePosition;
	private Vector3 startPosition;
	public bool shouldReturn;

	public void OnPointerDown(PointerEventData dt) {
		isMouseDown = true;

		Debug.Log ("Draggable Mouse Down");

		startPosition = target.position;
		startMousePosition = Mouse.current.position.ReadValue();
	}

	public void OnPointerUp(PointerEventData dt) {
		Debug.Log ("Draggable mouse up");

		isMouseDown = false;

		if (shouldReturn) {
			target.position = startPosition;
		}
	}

	// Update is called once per frame
	void Update () {
		if (isMouseDown) {
			Vector3 currentPosition = Mouse.current.position.ReadValue();

			Vector3 diff = currentPosition - startMousePosition;

			Vector3 pos = startPosition + diff;

			target.position = pos;
		}
	}
}