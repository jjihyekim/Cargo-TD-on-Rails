using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesOnBoostOrLowPower : MonoBehaviour {

    public bool isBoost = true;

    private void Start() {
        SetStatus(false);

        if (isBoost)
            GetComponentInParent<EngineModule>().OnEngineBoost.AddListener(SetStatus);
        else
            GetComponentInParent<EngineModule>().OnEngineLowPower.AddListener(SetStatus);
    }


    public void SetStatus(bool status) {
        if (status) {
            foreach (var particle in GetComponentsInChildren<ParticleSystem>()) {
                particle.Play();
            }
        } else {
            foreach (var particle in GetComponentsInChildren<ParticleSystem>()) {
                particle.Stop();
            }
        }
    }
}
