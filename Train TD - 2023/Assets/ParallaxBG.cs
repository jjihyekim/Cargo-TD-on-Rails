using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParallaxBG : MonoBehaviour {

    public float maxOffset = 10;
    
    

    void Update() {
        var mousePos = Mouse.current.position.ReadValue();
        mousePos = new Vector2(
            (mousePos.x / Screen.width) - 0.5f,
            (mousePos.y / Screen.height) -0.5f
            );
        GetComponent<RectTransform>().anchoredPosition = mousePos * maxOffset;
    }
}
