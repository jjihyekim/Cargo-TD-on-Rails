using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapPieceScript : MonoBehaviour {

	public float myAmount;
	public Color color1 = Color.white;
	public Color color2 = Color.grey;
	public void SetUpScrapPiece(float curScale,float maxScale, float minScale) {
		var scale = curScale.Remap(0, 1, minScale, maxScale);
		transform.localScale = Vector3.one*scale;
		myAmount = curScale;

		GetComponent<SpriteRenderer>().color = Color.Lerp(color1, color2, Random.Range(0f, 1f));
	}

}
