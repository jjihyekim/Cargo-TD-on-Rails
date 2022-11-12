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
    public List<GameObject> SpawnResourceAtLocation(ResourceTypes type, int amount, Vector3 location, bool autoCollect = true) {
        if (amount == 0)
            return null;
        
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

        var allPiles = new List<GameObject>();

        while(amount > 0) {
            var random = Random.insideUnitCircle * (count / 4f);
            var pile = Instantiate(prefab, location + new Vector3(random.x, 0, random.y), Quaternion.identity);
            var pileComp = pile.GetComponent<ScrapPile>();
            var targetAmount = maxMoneyPileCount;
            if (amount < maxMoneyPileCount)
                targetAmount = amount;
            pileComp.SetUp(targetAmount, type);
            allScraps.Add(pileComp);
            amount -= maxMoneyPileCount;
            
            if(autoCollect)
                pileComp.CollectPile();
            
            allPiles.Add(pile);
        }

        return allPiles;
    }
}
