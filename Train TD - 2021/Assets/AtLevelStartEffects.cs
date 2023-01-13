using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtLevelStartEffects : MonoBehaviour {
    

    public GameObject target;

    public bool isActiveBeforeStart = false;
    public bool isStarted = false;


    private void Start() {
        if (!SceneLoader.s.isLevelStarted())
            isActiveBeforeStart = true;
    }

    void Update()
    {
        if (!isActiveBeforeStart) {
            Destroy(gameObject);
        } else {
            if (SceneLoader.s.isLevelStarted()) {
                if (!isStarted) {
                    isStarted = true;
                    target.SetActive(true);
                    Destroy(gameObject, 5f);
                }
            }
        }
    }
}
