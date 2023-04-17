using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelWithFrontCart : MonoBehaviour
{
    // Update is called once per frame
    void Update() {
        if(SceneLoader.s.isLevelFinished())
            transform.position = Train.s.trainFront.position;
    }
}
