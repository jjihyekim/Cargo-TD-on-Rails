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
    public void SpawnScrapsAtLocation(int amount, Vector3 location) {
        int count = Mathf.CeilToInt(amount / 5f);

        while(amount > 0) {
            var random = Random.insideUnitCircle * (count / 4f);
            var pile = Instantiate(scrapPile, location + new Vector3(random.x, 0, random.y), Quaternion.identity);
            var pileComp = pile.GetComponent<ScrapPile>();
            var targetAmount = amount % maxMoneyPileCount;
            if (targetAmount == 0)
                targetAmount += maxMoneyPileCount;
            pileComp.SetUp(targetAmount, true);
            allScraps.Add(pileComp);
            amount -= 5;
            
            pileComp.CollectPile();
        }
    }
    
    public void SpawnFuelAtLocation(int amount, Vector3 location) {
        int count = Mathf.CeilToInt(amount / 5f);

        while(amount > 0) {
            var random = Random.insideUnitCircle * (count / 4f);
            var pile = Instantiate(fuelPile, location + new Vector3(random.x, 0, random.y), Quaternion.identity);
            var pileComp = pile.GetComponent<ScrapPile>();
            var targetAmount = amount % maxMoneyPileCount;
            if (targetAmount == 0)
                targetAmount += maxMoneyPileCount;
            pileComp.SetUp(targetAmount, false);
            allScraps.Add(pileComp);
            amount -= 5;
            
            pileComp.CollectPile();
        }
    }
}
