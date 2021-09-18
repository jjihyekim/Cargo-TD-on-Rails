using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Upgrade : MonoBehaviour {
    public string upgradeName = "unset";
    public bool isUnlocked = false;
    public int cost = 100;
    //public int starRequirement;

    public TMP_Text costText;
}
