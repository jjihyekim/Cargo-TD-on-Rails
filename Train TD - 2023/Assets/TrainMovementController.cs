using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainMovementController : MonoBehaviour {
    void Update() {
        transform.position += Vector3.forward * LevelReferences.s.speed * Time.deltaTime;
    }
}
