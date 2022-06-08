using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniGUI_DamageNumber : MonoBehaviour {
    public TMP_Text dmgText;
    private CanvasGroup canvasGroup;

    public float goUpSpeed = 1f;
    public float fadeOutStartTime = 1f;
    public float fadeOutEndTime = 2f;
    private Vector3 target;
    private Transform targetWithOffset;
    public void SetUp(Transform _target, int damage) {
        dmgText.text = "-" + damage;
        canvasGroup = GetComponent<CanvasGroup>();
        target = _target.position;

        targetWithOffset = Instantiate(new GameObject()).transform;
        targetWithOffset.transform.position = target;
        GetComponent<UIElementFollowWorldTarget>().SetUp(targetWithOffset);

        StartCoroutine(DamageAnimation());
    }


    IEnumerator DamageAnimation() {

        var timer = 0f;
        var curOffset = 0f;

        while (timer < fadeOutStartTime) {
            targetWithOffset.transform.position = target + Vector3.up * curOffset;

            curOffset += Time.deltaTime * goUpSpeed;
            timer += Time.deltaTime;

            yield return null;
        }

        var alpha = 1f;
        var alphaDelta = 1f/(fadeOutEndTime - fadeOutStartTime);
        while (timer < fadeOutEndTime) {
            targetWithOffset.transform.position = target + Vector3.up * curOffset;
            canvasGroup.alpha = alpha;

            curOffset += Time.deltaTime * goUpSpeed;
            timer += Time.deltaTime;
            alpha -= alphaDelta * Time.deltaTime;


            yield return null;
        }
        
        Destroy(gameObject);
    }
}
