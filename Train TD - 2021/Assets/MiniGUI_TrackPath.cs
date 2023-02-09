using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGUI_TrackPath : MonoBehaviour {
    public int trackId;
    
    public void TrackClicked() {
        PathSelectorController.s.ActivateLever(trackId);
    }
}
