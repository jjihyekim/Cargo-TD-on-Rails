using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MenuToggle : MonoBehaviour {

    public bool isMenuActive = false;
    public GameObject menu;
    public RectTransform menuRectArea;
    
    static GenericCallback  hideAllToggleMenus;


    [FormerlySerializedAs("singleMode")] public bool dontAutoCloseWithOtherPanels = false;


    public UnityEvent PanelEnabledEvent;
    public UnityEvent PanelDisabledEvent;
    

    private void OnEnable() {
        if(!dontAutoCloseWithOtherPanels)
            hideAllToggleMenus += HideMenu;
    }

    private void OnDisable() {
        if(!dontAutoCloseWithOtherPanels)
            hideAllToggleMenus -= HideMenu;
    }

    private void Start() {
        HideMenu();
    }


    private void Update() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (isMenuActive) {
                var rect1 = menuRectArea.GetComponent<RectTransform>();
                var rect1Val = RectTransformUtility.RectangleContainsScreenPoint(rect1, mousePos, OverlayCamsReference.s.uiCam);
                if (!rect1Val) {
                    HideMenu();
                }
            } else {
                /*var rect2 = GetComponent<RectTransform>();
                var rect2Val = RectTransformUtility.RectangleContainsScreenPoint(rect2, mousePos);
                if (rect2Val) {
                    ShowMenu();
                }*/
            }

        }
    }

    public static void HideAllToggleMenus () {
        hideAllToggleMenus?.Invoke();

        //SFX
        AudioManager.PlayOneShot(SfxTypes.ButtonClick2);
    }

    public void ToggleMenu() {
        if (isMenuActive) {
            HideMenu();
        } else {
            ShowMenu();
        }

        AudioManager.PlayOneShot(SfxTypes.ButtonClick1);
    }

    private GameObject previouslySelected;
    public void ShowMenu() {
        if(!dontAutoCloseWithOtherPanels)
            hideAllToggleMenus?.Invoke();
        isMenuActive = true;
        menu.SetActive(true);
        PanelEnabledEvent?.Invoke();

        if (SettingsController.GamepadMode()) {
            previouslySelected = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(menu.GetComponentInChildren<Button>().gameObject);
        }
    }

    public void HideMenu() {
        isMenuActive = false;
        menu.SetActive(false);
        PanelDisabledEvent?.Invoke();
        
        if (SettingsController.GamepadMode()) {
            if(previouslySelected != null)
                EventSystem.current.SetSelectedGameObject(previouslySelected);
        }
    }
}


public delegate void GenericCallback();
public delegate bool BoolReturnCallback();