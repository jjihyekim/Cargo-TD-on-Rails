using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility;

public class MiniGUI_HealthBar : MonoBehaviour{
    public Transform sourceTransform;
    private RectTransform CanvasRect;
    private RectTransform ParentRect;
    private RectTransform UIRect;
    private Camera mainCam;

    public ModuleHealth myModuleHealth;
    public ModuleAmmo myModuleAmmo;
    private bool isAmmoBar = false;

    public Slider slider;
    public Image filler;
    public Color healthyColor = Color.green;
    public Color deadColor = Color.red;
    
    
    
    public Slider ammoSlider;

    
    public void SetUp(ModuleHealth moduleHealth, ModuleAmmo moduleAmmo) {
        myModuleHealth = moduleHealth;
        
        myModuleAmmo = moduleAmmo;
        if (myModuleAmmo != null) {
            isAmmoBar = true;
        }
        ammoSlider.gameObject.SetActive(isAmmoBar);
        
        sourceTransform = myModuleHealth.GetComponent<TrainBuilding>().uiTargetTransform;
        CanvasRect = transform.root.GetComponent<RectTransform>();
        UIRect = GetComponent<RectTransform>();
        ParentRect = transform.parent.GetComponent<RectTransform>();
        mainCam = LevelReferences.s.mainCam;
        Update();
    }

    private void Update() {
        SetPosition();
        SetBarValue();
        SetAmmoBarValue();
    }
    
    private void LateUpdate() {
        AdjustPositionBasedOnOtherPositions();
    }

    private void SetPosition() {
        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint
        //treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.
        //SetOffset(); // for debugging
        Vector2 ViewportPosition = mainCam.WorldToViewportPoint(sourceTransform.position );
        Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));

        /*//now you can set the position of the ui element
        var halfWidthLimit = (CanvasRect.rect.width - (UIRect.rect.width + edgeGive)) / 2f;
        var halfHeightLimit = (CanvasRect.rect.height - (UIRect.rect.height + edgeGive)) / 2f;
        WorldObject_ScreenPosition.x = Mathf.Clamp(WorldObject_ScreenPosition.x,
            -halfWidthLimit,
            halfWidthLimit
        );
        WorldObject_ScreenPosition.y = Mathf.Clamp(WorldObject_ScreenPosition.y,
            -halfHeightLimit + (0.1f * CanvasRect.rect.height),
            halfHeightLimit
        );*/

        if(myVecRef != null)
            myVecRef.vector2 = WorldObject_ScreenPosition;
        UIRect.anchoredPosition = WorldObject_ScreenPosition;
    }
    
    class Vector2Reference {
        public Vector2 vector2;
    }
    
    static List<Vector2Reference> globalPositions = new List<Vector2Reference>();
    private Vector2Reference myVecRef;
    private void OnEnable() {
        myVecRef = new Vector2Reference();
        globalPositions.Add(myVecRef);
    }

    private void OnDisable() {
        globalPositions.Remove(myVecRef);
    }
    
    void AdjustPositionBasedOnOtherPositions() {
        for (int i = 0; i < globalPositions.Count; i++) {
            var curPos = globalPositions[i];
            if (curPos != myVecRef) {
                if (Vector2.Distance(curPos.vector2, myVecRef.vector2) < UIRect.sizeDelta.y) {
                    myVecRef.vector2.y += UIRect.sizeDelta.y + 0.1f;
                }
            }
        }

        Vector2 adjustedPosition = myVecRef.vector2;

        UIRect.anchoredPosition = adjustedPosition;
    }

    void SetBarValue() {
        var percent = myModuleHealth.currentHealth / myModuleHealth.maxHealth;
        percent = Mathf.Clamp(percent, 0, 1f);

        slider.value = percent;

        if (percent >= 1f) {
            if(slider.gameObject.activeSelf)
                slider.gameObject.SetActive(false);
        } else {
            if(!slider.gameObject.activeSelf)
                slider.gameObject.SetActive(true);
        }

        filler.color = Color.Lerp(deadColor, healthyColor, percent);
    }
    
    void SetAmmoBarValue() {
        if (isAmmoBar) {
            if (myModuleAmmo.ShowAmmoBar()) {
                var percent = (float)myModuleAmmo.curAmmo / myModuleAmmo.maxAmmo;
                percent = Mathf.Clamp(percent, 0, 1f);

                
                ammoSlider.value = percent;

                if(!ammoSlider.gameObject.activeSelf)
                    ammoSlider.gameObject.SetActive(true);
            } else {
                if(ammoSlider.gameObject.activeSelf)
                    ammoSlider.gameObject.SetActive(false);
            }
        }
    }
}
