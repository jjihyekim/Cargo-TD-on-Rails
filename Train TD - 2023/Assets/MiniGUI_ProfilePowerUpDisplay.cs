using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_ProfilePowerUpDisplay : MonoBehaviour {

    public Image background;
    public Image icon;

    public GameObject powerUpDisplay;
    
    public void UpdatePowerUpDisplay(string powerUpUniqueName) {
        if (powerUpUniqueName.Length > 0) {
            powerUpDisplay.SetActive(true);
            var powerUp = DataHolder.s.GetPowerUp(powerUpUniqueName);

            background.color = powerUp.color;
            icon.sprite = powerUp.icon;

            GetComponent<UITooltipDisplayer>().myTooltip.text = powerUp.description;
        } else {
            powerUpDisplay.SetActive(false);
        }
    }
}
