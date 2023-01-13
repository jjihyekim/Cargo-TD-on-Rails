using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlipMenu : MonoBehaviour
{
    public bool isMenuActiveAtStart = false;
    public GameObject menu;
    
    static GenericCallback  hideAllToggleMenus;

    private void Awake() {
        hideAllToggleMenus += HideMenu;
    }

    private void OnDestroy() {
        hideAllToggleMenus -= HideMenu;
    }

    private void Start() {
        if(isMenuActiveAtStart)
            ToggleMenu();
    }

    public void ToggleMenu() {
        ShowMenu();
    }

    void ShowMenu() {
        hideAllToggleMenus?.Invoke();
        menu.SetActive(true);
    }

    void HideMenu() {
        menu.SetActive(false);
    }
}
