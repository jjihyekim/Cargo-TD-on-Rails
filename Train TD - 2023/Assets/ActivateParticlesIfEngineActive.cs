using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateParticlesIfEngineActive : MonoBehaviour {

    public ParticleSystem[] particles;
    public EngineModule myModule;
    void Start()
    {
        /*for (int i = 0; i < particles.Length; i++) {
            particles[i].Stop();
        }*/
    }

    private bool lastState ;
    void Update()
    {
        /*if (lastState != myModule.hasFuel) {
            for (int i = 0; i < particles.Length; i++) {
                if (myModule.hasFuel) {
                    particles[i].Play();
                } else {
                    particles[i].Stop();
                }
            }

            lastState = myModule.hasFuel;
        }*/
    }
}
