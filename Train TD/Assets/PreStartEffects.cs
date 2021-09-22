using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreStartEffects : MonoBehaviour {

    public bool isDestroyStarted = false;
    
    
    private void Start() {
        // instant destroy if we begin with the level playing
        if (LevelLoader.s.isLevelStarted)
            DestroyNow();
    }
    
    void Update() {
        if (LevelLoader.s.isLevelStarted && ! isDestroyStarted) {
            isDestroyStarted = true;
            
            foreach (var particle in GetComponentsInChildren<ParticleSystem>()) {
                particle.Stop();
            }
            
            Invoke("DestroyNow", 1f);
        }
    }

    void DestroyNow() {
        Destroy(gameObject);
    }
}
