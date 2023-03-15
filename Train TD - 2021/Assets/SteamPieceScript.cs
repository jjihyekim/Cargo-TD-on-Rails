using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SteamPieceScript : MonoBehaviour
{
    
    public Vector2 randomInterval = new Vector2(0.1f,0.2f);
    public Vector2 randomForce = new Vector2(100, 200);

    public float repelForce = 50;

    private Rigidbody2D rg;

    public static List<SteamPieceScript> allPieces = new List<SteamPieceScript>();

    private void OnEnable() {
        allPieces.Add(this);
    }

    private void OnDisable() {
        allPieces.Remove(this);
    }

    void Start() {
        rg = GetComponent<Rigidbody2D>();
        //Invoke(nameof(ApplyJiggleForce), Random.Range(randomInterval.x, randomInterval.y));
    }

    private int maxCount = 50;
    private void FixedUpdate() {
        var iterateCount = Mathf.Min(maxCount,allPieces.Count);
        for (int i = 0; i < iterateCount; i++) {
            var forceVector = (transform.position - allPieces[i].transform.position);
            if (forceVector.sqrMagnitude < 0.1f) {
                var forceStrength = repelForce / Mathf.Max(forceVector.sqrMagnitude, 0.01f);
                allPieces[i].rg.AddForce(forceVector * forceStrength);
            }
        }
    }


    void ApplyJiggleForce() {
        //rg.AddForce(Random.insideUnitCircle.normalized*Random.Range(randomForce.x, randomForce.y), ForceMode2D.Impulse);
        
        //Invoke(nameof(ApplyJiggleForce), Random.Range(randomInterval.x, randomInterval.y));
    }
}
