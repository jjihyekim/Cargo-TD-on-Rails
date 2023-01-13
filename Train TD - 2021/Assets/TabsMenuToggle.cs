using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabsMenuToggle : MonoBehaviour {

    public GameObject myMenu;
    public Color activeColor = Color.blue;
    public Color selectableColor = Color.white;

    public Image myButtonImage;

    public bool defaultOpen = false;

    public static UnityEvent hideAllEvent = new UnityEvent();
    private void OnEnable() {
        hideAllEvent.AddListener(Hide);
        if (defaultOpen) {
            Show();
        } else {
            Hide();
        }
    }

    private void OnDisable() {
        hideAllEvent.RemoveListener(Hide);
    }

    public void Show() {
        hideAllEvent?.Invoke();
        myMenu.SetActive(true);
        myButtonImage.color = activeColor;
    }

    void Hide() {
        myMenu.SetActive(false);
        myButtonImage.color = selectableColor;
    }
}
