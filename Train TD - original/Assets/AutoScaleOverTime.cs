using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoScaleOverTime : MonoBehaviour {

    public float scaleSpeed = 0.2f;

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.one * (transform.localScale.x + (scaleSpeed*Time.deltaTime));
    }
}
