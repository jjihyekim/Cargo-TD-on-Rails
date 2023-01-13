using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCycleMove : MonoBehaviour
{
    public AnimationCurve xCurve;
    public float xMagnitude = 0;
    public AnimationCurve yCurve;
    public float yMagnitude = 0;
    public AnimationCurve zCurve;
    public float zMagnitude = 0;
    public float speed = 0.6f;

    private float curTime = 0;
    private Vector3 initialPos;

    private void Start() {
        initialPos = transform.position;
    }


    // Update is called once per frame
    void Update() {
        transform.position = initialPos + new Vector3(
            xCurve.Evaluate(curTime) * xMagnitude,
            yCurve.Evaluate(curTime) * yMagnitude,
            zCurve.Evaluate(curTime) * zMagnitude
        );
        curTime += Time.deltaTime * speed;
    }
}
