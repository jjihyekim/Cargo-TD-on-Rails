using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniGUI_DamageNumber : MonoBehaviour {
    public TMP_Text dmgText;
    private CanvasGroup canvasGroup;

    public float goUpSpeed = 1f;
    public float fadeOutStartTime = 1f;
    public float fadeOutEndTime = 2f;
    private Vector3 target;
    private Transform targetWithOffset;


    public int bigThreshold = 10;
    
    
    
    public string[] playerIsAttackedSmall;
    public string[] playerIsAttackedBig;
    
    
    public string[] enemyIsAttackedArmorProtected;

    public string[] enemyIsAttackedSmall;
    public string[] enemyIsAttackedBig;
    
    public Color armorProtectedColor = Color.yellow;
    public void SetUp(Transform _target, int damage, bool isPlayer, bool isArmorProtected) {
        //dmgText.text = "-" + damage;

        if (isPlayer)
        {
            if (isArmorProtected) {
                dmgText.text = enemyIsAttackedArmorProtected[Random.Range(0, enemyIsAttackedArmorProtected.Length)];
            } else {
                if (damage < bigThreshold) {
                    dmgText.text = enemyIsAttackedSmall[Random.Range(0, enemyIsAttackedSmall.Length)];
                } else {
                    dmgText.text = enemyIsAttackedBig[Random.Range(0, enemyIsAttackedBig.Length)];
                }
            }
        } else {
            if (damage < bigThreshold) {
                dmgText.text = playerIsAttackedSmall[Random.Range(0, playerIsAttackedSmall.Length)];
            } else {
                dmgText.text = playerIsAttackedBig[Random.Range(0, playerIsAttackedBig.Length)];
            }
        }


        if (isArmorProtected) {
            dmgText.color = armorProtectedColor;
        }
        
        canvasGroup = GetComponent<CanvasGroup>();
        target = _target.position;

        var obj = new GameObject("yeet");
        obj.name = "damage numbers tracking object";
        targetWithOffset = obj.transform;
        targetWithOffset.transform.position = target;
        GetComponent<UIElementFollowWorldTarget>().SetUp(targetWithOffset);

        StartCoroutine(DamageAnimation());
    }

    private void OnDestroy() {
        if (targetWithOffset != null)
            Destroy(targetWithOffset.gameObject);
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
