using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PulseAlpha : MonoBehaviour {

    public AnimationCurve curve;
    public float speed = 1f;

    private float curTime = 0;
    private Image _image;
    private Color _color;

    private void Start() {
        _image = GetComponent<Image>();
        _color = _image.color;
    }


    // Update is called once per frame
    void Update() {
        _color.a = curve.Evaluate(curTime);
        _image.color = _color;
        curTime += Time.deltaTime * speed;
    }
}
