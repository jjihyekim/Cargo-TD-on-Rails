using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SizeBasedOnZoomLevel : MonoBehaviour {

    
    [InfoBox("Every 1 unit of zoom change will result in multiplier amount of scale change")]
    public float zoomSizeChangeMultiplier = -0.5f;

    public float lerp = 0.5f;
    
    // 2 >> 0.5
    // 0 >> 0
    // -2 >> -0.5

    private float curScale = 1;
    void Update() {
        var wantedScale = (1 + CameraController.s.realZoom * zoomSizeChangeMultiplier);
        curScale = Mathf.Lerp(curScale, wantedScale, lerp * Time.deltaTime);
        transform.localScale = Vector3.one * curScale;
    }
}
