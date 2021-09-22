using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Upgrade : MonoBehaviour {
    public string upgradeName = "unset";
    public bool isUnlocked = false;
    public int cost = 100;
    //public int starRequirement;
}
