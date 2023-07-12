using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_StatusChangedEffect : MonoBehaviour {

    public float growSpeed = 10;
    public float lifetime = 0.5f;
    // Start is called before the first frame update
    void Start() {
	    StartCoroutine(GrowAndDie());
    }

    IEnumerator GrowAndDie() {
	    var image = GetComponent<Image>();
	    var rect = GetComponent<RectTransform>();
	    var size = rect.sizeDelta.x;
	    var color = image.color;
	    var alpha = 1f;
	    var alphaDecaySpeed = 1/(lifetime / 2f);
	    
	    var timer = 0f;

	    while (timer < lifetime/2f) {
		    rect.sizeDelta = Vector2.one*size;

		    size += growSpeed * Time.deltaTime;

		    timer += Time.deltaTime;
		    yield return null;
	    }
	    
	    while (timer < lifetime) {
		    rect.sizeDelta = Vector2.one*size;
		    image.color = color;

		    size += growSpeed * Time.deltaTime;
		    alpha -= alphaDecaySpeed * Time.deltaTime;
		    color.a = alpha;

		    timer += Time.deltaTime;
		    yield return null;
	    }
	    
	    Destroy(gameObject);
    }
}
