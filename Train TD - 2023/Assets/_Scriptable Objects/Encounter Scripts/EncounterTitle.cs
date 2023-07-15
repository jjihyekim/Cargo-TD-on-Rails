using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterTitle : MonoBehaviour {
    [Tooltip("Make sure this is unique")]
    public string title = "Unset Encounter";
    public Sprite image;

    public EncounterNode initialNode;

    public Transform encounterCam;

    [NonSerialized]
    public string doYouWantToStopText = "You see something interesting by the side of the rails";
    [NonSerialized]
    public float autoRidePastTimer = 10f;

    public float ambushChance = 0.3f;

    //public LevelSegmentScriptable ridePastAmbush;
}
