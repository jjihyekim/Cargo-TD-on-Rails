using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomParticleTurnOnAndOff : MonoBehaviour {

    public Vector2 stayOnTime = new Vector2(0.5f, 1.5f);

    public Vector2 stayOfftime = new Vector2(2f, 4f);
    
    void Start()
    {
        Invoke("TurnOn",Random.Range(0.2f,4f));
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
