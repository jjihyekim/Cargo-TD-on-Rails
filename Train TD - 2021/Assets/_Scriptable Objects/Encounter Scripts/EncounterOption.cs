using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterOption : MonoBehaviour {

    
    public bool optionEnabled = true;
    
    [Multiline(4)] 
    public string text;

    public EncounterNode nextNode;
}
