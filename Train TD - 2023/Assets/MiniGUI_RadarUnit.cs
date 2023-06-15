using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_RadarUnit : MonoBehaviour {
    public Image icon;
    public Image top;
    public Image bottom;
    public Image background;

    public float percentage;
    private float baseHeight => DistanceAndEnemyRadarController.s.baseHeight;
    public void SetUp(Sprite _icon, bool isLeft, float percentDistance) {
        if (percentDistance < 0) {
            this.enabled = false;
        }
        percentage = percentDistance;
        icon.sprite = _icon;

        top.gameObject.SetActive(isLeft);
        bottom.gameObject.SetActive(!isLeft);
        if (isLeft) {
            background.color = LevelReferences.s.leftColor;
            top.color = LevelReferences.s.leftColor;
        } else {
            background.color = LevelReferences.s.rightColor;
            bottom.color = LevelReferences.s.rightColor;
        }
        
    }

    public void SetUp(Sprite _icon, float percentDistance, bool isElite, bool isEncounter) { // for the train or for stuff with no sides
        percentage = percentDistance;
        icon.sprite = _icon;
        
        top.gameObject.SetActive(false);
        bottom.gameObject.SetActive(false);
        
        background.color = Color.white;

        if (isElite)
            background.color = LevelReferences.s.eliteColor;
        if (isEncounter)
            background.color = LevelReferences.s.encounterColor;
    }

    private void LateUpdate() {
        var distance = percentage * transform.parent.GetComponent<RectTransform>().rect.width;
        GetComponent<RectTransform>().anchoredPosition = new Vector2(distance, baseHeight);
    }
}
