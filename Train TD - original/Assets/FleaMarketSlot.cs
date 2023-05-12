using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleaMarketSlot : MonoBehaviour {
    public bool isOccupied = false;

    public float rotationSpeed = 2f;
    public Transform GetLocation() {
        return transform.GetChild(0);
    }

    private void Update() {
        transform.Rotate(0,rotationSpeed*Time.deltaTime,0);
    }


    public GameObject emptyShowObject;
    public void SetEmptyStatus(bool isEmpty) {
        emptyShowObject.SetActive(isEmpty);
    }
}
