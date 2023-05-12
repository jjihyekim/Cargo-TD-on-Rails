using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomParticleTurnOnAndOff : MonoBehaviour {

    public Vector2 stayOnTime = new Vector2(0.5f, 1.5f);

    public Vector2 stayOfftime = new Vector2(2f, 4f);
    
    void OnEnable()
    {
        Invoke("TurnOn",Random.Range(0.2f,4f));
    }

    private void OnDisable() {
        CancelInvoke();
        GetComponent<ParticleSystem>().Stop();
    }

    public void TurnOff() {
        GetComponent<ParticleSystem>().Stop();
        
        Invoke("TurnOn",Random.Range(stayOfftime.x,stayOfftime.y));
    }

    public void TurnOn() {
        GetComponent<ParticleSystem>().Play();
        
        Invoke("TurnOff",Random.Range(stayOnTime.x,stayOnTime.y));
    }
}
