using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ModuleStorage : MonoBehaviour, IActiveDuringCombat, IActiveDuringShopping {
    public ResourceTypes myType;
    public int amount = 100;
    public float generationPerSecond = 0.1f;

    private void OnEnable() {
        var train = GetComponentInParent<Train>();
        
        if(train != null)
            train.ReCalculateStorageAmounts();
    }


    private int chunkAmount = 5;
    private float curAmount = 0;

    private bool generateMaterials = false;
    private void Update() {
        if (generateMaterials) {
            curAmount += generationPerSecond * Time.deltaTime;
            if (curAmount > chunkAmount) {
                curAmount -= chunkAmount;
                
                var pile = LevelReferences.s.SpawnResourceAtLocation(myType, chunkAmount, transform.position, false);
                StartCoroutine(ThrowPile(pile));
            }
        }
    }

    private float throwHorizontalSpeed = 0.1f;
    private float throwVerticalSpeed = 0.2f;
    private float throwGravity = 9.81f;
    IEnumerator ThrowPile(List<GameObject> pile) {

        var random = Random.insideUnitCircle.normalized;
        var direction = new Vector3(random.x, 0, random.y);
        var verticalSpeed = throwVerticalSpeed;

        while (verticalSpeed > -throwVerticalSpeed) {
            for (int i = 0; i < pile.Count; i++) {
                pile[i].transform.position += (direction * throwHorizontalSpeed + Vector3.up * verticalSpeed) * Time.deltaTime;
            }

            verticalSpeed -= throwGravity * Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < pile.Count; i++) {
            pile[i].GetComponent<ScrapPile>().CollectPile();
        }
    }

    private void OnDestroy() {
        var train = GetComponentInParent<Train>();
        
        if(train != null)
            train.ReCalculateStorageAmounts();
    }

    public void ActivateForCombat() {
        this.enabled = true;
        generateMaterials = true;
    }

    public void ActivateForShopping() {
        this.enabled = true;
        generateMaterials = false;
    }

    public void Disable() {
        this.enabled = false;
    }
}
