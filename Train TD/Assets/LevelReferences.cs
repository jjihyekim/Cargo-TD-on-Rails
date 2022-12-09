using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelReferences : MonoBehaviour {

    public static LevelReferences s;

    public Transform playerTransform {
        get {
            return transform;
        }
    }

    public Camera mainCam {
        get {
            return MainCameraReference.s.cam;
        }
    }

    public GameObject metalBulletHitEffectPrefab;
    public GameObject dirtBulletHitEffectPrefab;
    public GameObject enemyRegularHitEffectPrefab;
    public GameObject enemyCantPenetrateHitEffectPrefab;
    public GameObject rocketExplosionEffectPrefab;
    public GameObject laserHitPrefab;
    public GameObject mortarExplosionEffectPrefab;
    public GameObject mortarMiniHitPrefab;
    public GameObject waveDisplayPrefab;
    public GameObject partHealthPrefab;
    public GameObject damageNumbersPrefab;
    public Transform uiDisplayParent;

    public float speed = 1f;

    public static List<PossibleTarget> allTargets = new List<PossibleTarget>();

    public GameObject scrapPile;
    public GameObject fuelPile;
    public GameObject ammoPile;

    public static List<ScrapPile> allScraps = new List<ScrapPile>();
    public Train train;

    
    public Material hologramBuildable;
    public Material hologramCantBuild;
    
    
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public LayerMask buildingLayer;
    
    private void Awake() {
        s = this;
    }


    const int maxMoneyPileCount = 25;

    public void SpawnResourceAtLocation(ResourceTypes type, float amount, Vector3 location) {
        SpawnResourceAtLocation(type, Mathf.RoundToInt(amount), location);
    }
    public void SpawnResourceAtLocation(ResourceTypes type, int amount, Vector3 location) {
        if (amount == 0)
            return;
        
        int count = Mathf.CeilToInt(amount / (float)maxMoneyPileCount);

        GameObject prefab = null;

        switch (type) {
            case ResourceTypes.ammo:
                prefab = ammoPile;
                break;
            case ResourceTypes.fuel:
                prefab = fuelPile;
                break;
            case ResourceTypes.scraps:
                prefab = scrapPile;
                break;
        }

        while(amount > 0) {
            //var random = Random.insideUnitCircle * (count / 4f);
            var pile = Instantiate(prefab, location /*+ new Vector3(random.x, 0, random.y)*/, Quaternion.identity);
            var pileComp = pile.GetComponent<ScrapPile>();
            var targetAmount = maxMoneyPileCount;
            if (amount < maxMoneyPileCount)
                targetAmount = amount;
            pileComp.SetUp(targetAmount, type);
            amount -= maxMoneyPileCount;

            StartCoroutine(ThrowPile(pile));
        }
    }
    
    
    public float throwHorizontalSpeed = 0.1f;
    public float throwVerticalSpeed = 0.2f;
    public float throwGravity = 9.81f;
    IEnumerator ThrowPile(GameObject pile) {
        var random = Random.insideUnitCircle.normalized;
        var direction = new Vector3(random.x, 0, random.y);
        var verticalSpeed = throwVerticalSpeed;

        while (verticalSpeed > -throwVerticalSpeed) {
            pile.transform.position += (direction * throwHorizontalSpeed + Vector3.up * verticalSpeed) * Time.deltaTime;
            

            verticalSpeed -= throwGravity * Time.deltaTime;
            yield return null;
        }

        pile.GetComponent<ScrapPile>().CollectPile();
    }
}
