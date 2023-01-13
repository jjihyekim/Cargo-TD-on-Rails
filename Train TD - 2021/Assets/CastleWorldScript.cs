using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleWorldScript : MonoBehaviour {

    public Outline outline;
    
    public Color travelableColor = Color.magenta;
    public Color playerHereColor = Color.cyan;

    public Transform gfxParent;
    
    public StarState myInfo;

    private bool isOutlinePermanentOn = false;
    private float defaultOutlineWidth;
    
    public GameObject playerIndicator;
    public void SetHighlightState(bool isOpen) {
        if (isOutlinePermanentOn) {
            if (isOpen) {
                outline.OutlineWidth = defaultOutlineWidth * 2f;
            } else {
                outline.OutlineWidth = defaultOutlineWidth;
            }
        } else {
            outline.enabled = isOpen;
        }
    }
    
    public void Initialize(StarState info) {
        myInfo = info;

        Instantiate(DataHolder.s.GetCityPrefab(info.city.nameSuffix), gfxParent);
        outline = GetComponentInChildren<Outline>();
        
        playerIndicator.SetActive(false);

        /*if(myInfo.isPlayerHere)
            SetPlayerHere();*/
    }


    public void SetPlayerHere() {
        playerIndicator.SetActive(true);
        outline.OutlineColor = playerHereColor;
        //outline.enabled = true;
        //isOutlinePermanentOn = true;
        defaultOutlineWidth = outline.OutlineWidth;
    }

    public void SetTravelable(bool currentlyTravelable) {
        if (currentlyTravelable) {
            outline.OutlineColor = travelableColor;
            //outline.enabled = true;
            //isOutlinePermanentOn = true;
            defaultOutlineWidth = outline.OutlineWidth;
        }
    }

}
