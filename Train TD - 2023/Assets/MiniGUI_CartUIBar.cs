using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_CartUIBar : MonoBehaviour {

    public Image icon;

    public RectTransform mainRect;
    
    public RectTransform ammoBar;
    public RectTransform healthBar;

    public RectTransform directControl;
    public RectTransform engineBoost;


    public Cart myCart;
    public ModuleHealth myHealth;
    public ModuleAmmo myAmmo;

    public bool isAmmo = false;
    
    public Color fullColor = Color.green;
    public Color halfColor = Color.yellow;
    public Color emptyColor = Color.red;

    public float ammoMinHeight;
    public float ammoMaxHeight;

    
    public float boostMinHeight;
    public float boostMaxHeight;
    
    public float cartLengthToWidth = 100;

    public GameObject warning;

    public bool showWarning = false;

    public bool isBoost = false;

    public void SetUp(Cart cart, ModuleHealth moduleHealth, ModuleAmmo moduleAmmo) {
        myCart = cart;
        myHealth = moduleHealth;
        myAmmo = moduleAmmo;
        isAmmo = myAmmo != null;
        var boostable = cart.GetComponentInChildren<EngineBoostable>();
        isBoost = boostable != null;
        ammoBar.gameObject.SetActive(isAmmo);
        directControl.gameObject.SetActive(cart.GetComponentInChildren<DirectControllable>() != null);
        engineBoost.gameObject.SetActive(isBoost);
        
        mainRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cartLengthToWidth*myCart.length);
        Update();

        icon.sprite = myCart.Icon;
        UpdateAllSiblingPositions();
        
        warning.SetActive(false);

        showWarning = myCart.isCriticalComponent || myCart.isMainEngine;
    }

    void UpdateAllSiblingPositions() {
        var allBars = transform.parent.GetComponentsInChildren<MiniGUI_CartUIBar>();

        for (int i = 0; i < allBars.Length; i++) {
            allBars[i].transform.SetSiblingIndex((allBars.Length - 1) - allBars[i].myCart.trainIndex);
        }
    }

    private void Update() {
        SetHealthBarValue();
        if (isAmmo)
            SetAmmoBarValue();
        if (isBoost)
            SetBoostBarValue();
    }

    void SetHealthBarValue() {
        var percent = myHealth.GetHealthPercent();
        percent = Mathf.Clamp(percent, 0, 1f);

        var totalLength = mainRect.sizeDelta.x;
        
        healthBar.SetRight(totalLength*(1-percent));
        healthBar.GetComponent<Image>().color = GetHealthColor(percent);

        if (showWarning) {
            warning.SetActive(percent < 0.5f);
            warning.GetComponent<PulseAlpha>().speed = percent.Remap(0f, 0.5f, 3f, 0.2f);
        }
    }

    void SetAmmoBarValue() {
        var percent = myAmmo.AmmoPercent();
        percent = Mathf.Clamp(percent, 0, 1f);
        
        ammoBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(ammoMinHeight, ammoMaxHeight, percent));
    }
    
    void SetBoostBarValue() {
        var percent = SpeedController.s.boostGraphicPercent;
        engineBoost.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, percent.Remap(0,1,boostMinHeight, boostMaxHeight));
    }
    
    private Color GetHealthColor(float percentage) {
        Color color;
        if (percentage > 0.5f) {
            color = Color.Lerp(halfColor, fullColor, (percentage - 0.5f) * 2);
        } else {
            color = Color.Lerp(emptyColor, halfColor, (percentage) * 2);
        }

        return color;
    }

    [Button]
    public void DebugSetHP(float hp) {
        var percent = hp;
        var totalLength = mainRect.sizeDelta.x;
        
        healthBar.SetRight(totalLength*(1-percent));
        healthBar.GetComponent<Image>().color = GetHealthColor(percent);
    }
    
    [Button]
    public void DebugSetAmmo(float ammo) {
        var percent = ammo;
        
        ammoBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(ammoMinHeight, ammoMaxHeight, percent));
    }

    public void ClickRepair() {
        PlayerWorldInteractionController.s.UIRepair(myCart);
    }

    public void ClickReloadOrDirectControl() {
        PlayerWorldInteractionController.s.UIReloadOrDirectControlOrBoost(myCart);
    }
}
