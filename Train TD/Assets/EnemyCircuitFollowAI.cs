using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityStandardAssets.Utility;

public class EnemyCircuitFollowAI : MonoBehaviour {
    
    public WaypointCircuit myPath;
    public bool isLeft = false; //left is the default position, and right is mirrored

    public float speed = 1f;

    public float curDistance = 0;
    
    public bool goingBack = false;

    private bool isTimerSet = false;
    private float timer = 0;

    private Vector3 targetPos;
    private void Start() {
        targetPos = transform.position;
    }

    void Update() {
        if ((curDistance < myPath.Length || myPath.loop) && ! goingBack) {

            targetPos = myPath.GetRoutePosition(curDistance, !isLeft);

            curDistance += speed * Time.deltaTime;
        } else if (myPath.pingPong) {
            goingBack = true;

            if (!isTimerSet) {
                isTimerSet = true;
                timer = myPath.waitAtTheEndSeconds;
            }
            
            timer -= Time.deltaTime;

            if (timer > 0) {
                // wait
            } else if (curDistance > 0) { // go back

                targetPos = myPath.GetRoutePosition(curDistance, !isLeft);

                curDistance -= speed * Time.deltaTime;
            } else {
                Destroy(gameObject);
            }
        }else {
            Destroy(gameObject);
        }

        

        transform.position = targetPos;
    }

    [Button()]
    public void SnapToCurDistance() {
        targetPos = myPath.GetRoutePosition(curDistance, !isLeft);
        
        transform.position = targetPos;
    }

    public Vector3 GetFurtherPositionInCircuit(float time) {
        var multiplier = 1f;
        if (goingBack)
            multiplier = -1f;
        return myPath.GetRoutePosition(curDistance + speed*time*multiplier, !isLeft);
    }
}
