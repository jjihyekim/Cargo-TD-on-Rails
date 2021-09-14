using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityStandardAssets.Utility;

public class EnemyCircuitFollowAI : MonoBehaviour {
    
    public WaypointCircuit myPath;
    public bool isLeft = false;

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

            targetPos = myPath.GetRoutePosition(curDistance);
            if (!isLeft) {
                targetPos.x = -targetPos.x;
            }

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

                targetPos = myPath.GetRoutePosition(curDistance);
                if (!isLeft) {
                    targetPos.x = -targetPos.x;
                }

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
        targetPos = myPath.GetRoutePosition(curDistance);
        if (!isLeft) {
            targetPos.x = -targetPos.x;
        }
        
        transform.position = targetPos;
    }
}
