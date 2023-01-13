using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterTitle : MonoBehaviour {
    [Tooltip("Make sure this is unique")]
    public string title = "Unset Encounter";
    public Sprite image;

    public EncounterNode initialNode;

    public Transform encounterCam;
}
