using System;
using UnityEngine;

public class HexCell : MonoBehaviour {

	public HexCoordinates coordinates;

	public Vector3 expectedCoordinates;

	/*private void Update() {
		/*coordinates = HexCoordinates.PositionToHex(transform.position);

		if (!Input.GetMouseButton(0)) {
			transform.position = HexCoordinates.HexToPosition(coordinates, transform.position.y);
		}#1#
	}*/
	
	
/*#if UNITY_EDITOR
	[MethodButton("SetPosition")]
	[SerializeField] private bool editorFoldout;
#endif*/
}