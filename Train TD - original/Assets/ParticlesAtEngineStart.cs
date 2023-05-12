using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesAtEngineStart : MonoBehaviour
{
    private void Start() {
        if (PlayStateMaster.s.isShop()) {
            Activate();
        } else {
            Disable();
        }
        
        GetComponentInParent<EngineModule>().OnEngineStart.AddListener(Activate);
    }


    void Activate() {
        foreach (var particle in GetComponentsInChildren<ParticleSystem>()) {
            particle.Play();
        }

        foreach (var audio in GetComponentsInChildren<AudioSource>()) {
            audio.Play();
        }
        
        
    }

    void Disable() {
        foreach (var particle in GetComponentsInChildren<ParticleSystem>()) {
            particle.Stop();
        }
    }

}
