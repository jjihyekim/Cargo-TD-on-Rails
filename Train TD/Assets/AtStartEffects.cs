using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtStartEffects : MonoBehaviour {

    public GameObject target;

    public bool isActiveBeforeStart = false;
    public bool isStarted = false;


    private void Start() {
        if (!LevelLoader.s.isLevelStarted)
            isActiveBeforeStart = true;
    }

    void Update()
    {
        if (!isActiveBeforeStart) {
            Destroy(gameObject);
        } else {
            if (LevelLoader.s.isLevelStarted) {
                if (!isStarted) {
                    isStarted = true;
                    target.SetActive(true);
                    Destroy(gameObject, 5f);
                }
            }
        }
    }
}
