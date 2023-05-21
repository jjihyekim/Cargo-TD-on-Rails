using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MiniGUI_TutorialHint : MonoBehaviour {
    private GameObject hint;
    protected Cart target;

    protected void SetStatus(bool isActive) {
        hint.SetActive(isActive);
    }

    public GameObject SetUp(Cart _target) {
        target = _target;
        hint = transform.GetChild(0).gameObject;
        GetComponentInChildren<UIElementFollowWorldTarget>().SetUp(target.uiTargetTransform);
        SetStatus(false);
        _SetUp();
        return gameObject;
    }

    protected virtual void _SetUp() { }
    
    void Update()
    {
        _Update();
    }

    protected abstract void _Update();
}
