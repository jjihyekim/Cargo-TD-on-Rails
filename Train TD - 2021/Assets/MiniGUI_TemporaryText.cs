using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class MiniGUI_TemporaryText : MonoBehaviour {

    public float fadeStartTime = 0.5f;
    public float fadeDuration = 0.5f;
    public float throwSpeedX = 10f;
    public float throwSpeedY = 20f;
    public float gravity = 10;

    void Start() {
        StartCoroutine(Fade());
        StartCoroutine(Animation());
    }

    IEnumerator Fade() {
        var canvasGroup = GetComponent<CanvasGroup>();

        yield return new WaitForSeconds(fadeStartTime);

        var alpha = 1f;
        var alphaDelta = 1f / (fadeDuration);
        while (alpha > 0) {
            canvasGroup.alpha = alpha;
            alpha -= alphaDelta * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator Animation() {
        var xDelta = Random.Range(-throwSpeedX, throwSpeedX);
        var yDelta = throwSpeedY;

        while (true) {
            transform.position += new Vector3(xDelta, yDelta, 0) * Time.deltaTime;
            yDelta -= gravity * Time.deltaTime;
            yield return null;
        }
    }
}
