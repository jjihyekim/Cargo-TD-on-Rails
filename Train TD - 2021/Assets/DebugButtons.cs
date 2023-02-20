using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class DebugButtons : MonoBehaviour
{
    
    
    [Button]
    public void Strecth() {
        var _parent = transform.parent.GetComponent<RectTransform>();
        var _mRect = GetComponent<RectTransform>();
        _mRect.anchorMin = Vector2.zero;
        _mRect.anchorMax = Vector2.one;
        _mRect.pivot = new Vector2(0, 0.5f);
        _mRect.sizeDelta = Vector2.zero;
        _mRect.anchoredPosition = Vector2.zero;
    
        //_mRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _parent.rect.width);
        //_mRect.transform.SetParent(_parent);
    }

    [Button]
    public void RectWidht() {
        print(GetComponent<RectTransform>().rect.width);
    }
}
