using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartDestroy : MonoBehaviour
{
    public void Engage() {
        var particles = GetComponentsInChildren<ParticleSystem>();

        foreach (var particle in particles) {
            //particle.transform.SetParent(null);
            particle.Stop();
            //Destroy(particle.gameObject, 5f);
        }
        
        Destroy(gameObject,5f);
    }
}
