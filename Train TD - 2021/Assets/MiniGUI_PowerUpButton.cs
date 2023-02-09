using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiniGUI_PowerUpButton : MonoBehaviour {

    public Image activePowerUp;
    public GameObject deleteButton;
    public Image powerUpIcon;


    public PowerUpScriptable myPowerUp;
    public void OnClick() {
        PlayerActionsController.s.ClickPowerUpButton(this);
    }

    public void OnDeleteClick() {
        PlayerActionsController.s.ClickPowerUpButtonDelete(this);
    }

    public void SetPowerUp(PowerUpScriptable powerUpScriptable) {
        myPowerUp = powerUpScriptable;
        activePowerUp.gameObject.SetActive(true);
        deleteButton.SetActive(true);
        powerUpIcon.sprite = myPowerUp.icon;
        activePowerUp.color = myPowerUp.color;

        GetComponentInChildren<UITooltipDisplayer>().myTooltip.text = powerUpScriptable.description;

    }

    public void Clear() {
        activePowerUp.gameObject.SetActive(false);
        deleteButton.SetActive(false);
    }
}
