using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapPieceScript : MonoBehaviour {

	public float myAmount;
	public Color color1 = Color.white;
	public Color color2 = Color.grey;

	public GameObject poofOnDestroy;
	public void SetUpScrapPiece(float amount, float maxAmount, float maxScale, float minScale) {
		var scale = amount / maxAmount;
		scale = scale.Remap(0, 1, minScale, maxScale);
		transform.localScale = Vector3.one*scale;
		myAmount = amount;

		GetComponent<SpriteRenderer>().color = Color.Lerp(color1, color2, Random.Range(0f, 1f));
	}

	public void DestroySelf() {
		var part = Instantiate(poofOnDestroy, transform.position, Quaternion.identity).GetComponentInChildren<ParticleSystem>();
		part.transform.parent.SetParent(transform.parent);
		var main = part.main;
		main.startColor = GetComponent<SpriteRenderer>().color;
		Destroy(gameObject);
	}

}
