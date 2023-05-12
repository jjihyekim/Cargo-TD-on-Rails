using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldFollowerSorter : MonoBehaviour {
    public static List<UIElementFollowWorldTarget> activeElements = new List<UIElementFollowWorldTarget>();
    public static List<UIElementFollowWorldTarget> avoidanceElements = new List<UIElementFollowWorldTarget>();
    
    private void LateUpdate() {
        /*for (int i = 0; i < activeElements.Count; i++) {
            activeElements[i].SetPosition();
        }
        
        SortElements();*/
    }

    void SortElements() {
        avoidanceElements.Sort((x,y) => x.UIRect.anchoredPosition.y.CompareTo(y.UIRect.anchoredPosition.y));
        
        for (int i = 1; i < avoidanceElements.Count; i++) {
                if (RectOverlaps(avoidanceElements[i-1].UIRect, avoidanceElements[i].UIRect)) {
                    var pos = avoidanceElements[i].UIRect.anchoredPosition;
                    pos.y += avoidanceElements[i - 1].UIRect.sizeDelta.y + 0.1f;
                    avoidanceElements[i].UIRect.anchoredPosition = pos;
                }
            
        }
    }
    
    bool RectOverlaps(RectTransform rectTrans1, RectTransform rectTrans2)
    {
        Rect rect1 = new Rect(rectTrans1.localPosition.x, rectTrans1.localPosition.y, rectTrans1.rect.width, rectTrans1.rect.height);
        Rect rect2 = new Rect(rectTrans2.localPosition.x, rectTrans2.localPosition.y, rectTrans2.rect.width, rectTrans2.rect.height);

        return rect1.Overlaps(rect2);
    }
}
