using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceParticleScript : MonoBehaviour {

    public float lifetime = 2f;
    public GameObject forceField;
    public void SetUp(Transform target) {
        forceField.transform.SetParent(target);
        forceField.transform.localPosition = Vector3.zero;
        forceField.transform.LookAt(transform.position);
        
        Invoke(nameof(DestroySelf),lifetime);
    }


    void DestroySelf() {
        Destroy(gameObject);
        Destroy(forceField);
    }
}
