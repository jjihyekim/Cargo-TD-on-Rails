using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtRegularStartEffects : MonoBehaviour
{
    public GameObject target;


    private void Start() {
        // only engage if this is after the level has begun
        if (!LevelLoader.s.isLevelStarted) {
            Destroy(gameObject);
        } else {
            target.SetActive(true);
            Destroy(gameObject, 5f);
        }
    }

}
