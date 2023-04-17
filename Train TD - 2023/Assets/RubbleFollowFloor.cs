using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleFollowFloor : MonoBehaviour {
    public float constantForce = 10f;

    public float deathTime = 20f;


    private void Start() {
        Invoke("DestroyNow", deathTime);
    }


    void DestroyNow() {
        Destroy(gameObject);
    }

    private void FixedUpdate() {
        GetComponent<Rigidbody>().AddForce(Vector3.back * constantForce);
    }
    
    
    private void OnCollisionEnter(Collision other) {
        if (other.transform.parent != null && other.transform.parent.CompareTag("Ground")) {
            transform.SetParent(other.transform);
            other.gameObject.GetComponentInParent<HexChunk>().foreignObjects.Add(gameObject);
        }
    }

    /*private void OnCollisionExit(Collision other) {
        if (other.gameObject.tag == "Ground") {
            transform.SetParent(null);
        }
    }*/
}
