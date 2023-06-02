using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject enemyHealthPrefab;
    public GameObject cartHealthPrefab;
    public Transform cartHealthParent;
    public GameObject repairEffectPrefab;
    public GameObject reloadEffectPrefab;
    public GameObject damageNumbersPrefab;
    public Transform uiDisplayParent;

    public GameObject smallDamagePrefab;
    public GameObject mediumDamagePrefab;
    public GameObject bigDamagePrefab;

    public GameObject buildingHPLowParticles;
    public GameObject buildingHPCriticalParticles;
    public GameObject buildingDestroyedParticles;

    public GameObject teleportStartEffect;
    public float teleportTime = 1f;
    public GameObject teleportCompleteEffect;

    public GameObject currentlySlowedEffect;

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
    public LayerMask cartSnapLocationsLayer;
    public LayerMask gateMask;

    public SingleUnityLayer playerBulletLayer;
    public SingleUnityLayer enemyBulletLayer;
    
    public Color leftColor = Color.white;
    public Color rightColor = Color.white;

    public Sprite encounterIcon;
    public Sprite smallEnemyIcon;
    public Sprite eliteEnemyIcon;
    public Sprite bossEnemyIcon;

    public GameObject resourceParticleScraps;
    public GameObject resourceParticleAmmo;
    public GameObject resourceParticleFuel;

    public GameObject enemyCartReward;
    
    
    public GameObject resourceLostParticleScraps;
    public GameObject resourceLostParticleAmmo;
    public GameObject resourceLostParticleFuel;

    public GameObject emptyCart;

    public GameObject noAmmoWarning;

    public GameObject GetResourceParticle(ResourceTypes types) {
        switch (types) {
            case ResourceTypes.scraps:
                return resourceParticleScraps;
            default:
                return null;
        }
    }
    
    public GameObject GetResourceLostParticle(ResourceTypes types) {
        switch (types) {
            case ResourceTypes.scraps:
                return resourceLostParticleScraps;
            default:
                return null;
        }
    }
    
    private void Awake() {
        s = this;
        
    }

    const int maxMoneyPileCount = 5;

    public void SpawnResourceAtLocation(ResourceTypes type, float amount, Vector3 location) {
        return;
        SpawnResourceAtLocation(type, Mathf.RoundToInt(amount), location);
    }
    public void SpawnResourceAtLocation(ResourceTypes type, int amount, Vector3 location, bool customCollectTarget = false, Transform customCollectTargetTransform = null) {
        if (amount == 0)
            return;
        
        int count = Mathf.CeilToInt(amount / (float)maxMoneyPileCount);

        GameObject prefab = null;

        switch (type) {
            case ResourceTypes.scraps:
                prefab = scrapPile;
                break;
        }

        if (prefab == null) {
            return;
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

            StartCoroutine(ThrowPile(pile, customCollectTarget, customCollectTargetTransform));
        }
    }
    
    
    public float throwHorizontalSpeed = 0.1f;
    public float throwVerticalSpeed = 0.2f;
    public float throwGravity = 9.81f;

    IEnumerator ThrowPile(GameObject pile, bool customCollectTarget = false, Transform customCollectTargetTransform = null) {
        var random = Random.insideUnitCircle.normalized;
        var direction = new Vector3(random.x, 0, random.y);
        if (customCollectTarget) {
            direction =  customCollectTargetTransform.position - pile.transform.position;
            direction.y = 0;
        }
        
        direction.Normalize();
        var verticalSpeed = throwVerticalSpeed;

        while (verticalSpeed > -throwVerticalSpeed) {
            pile.transform.position += (direction * throwHorizontalSpeed + Vector3.up * verticalSpeed) * Time.deltaTime;


            verticalSpeed -= throwGravity * Time.deltaTime;
            yield return null;
        }

        if (!customCollectTarget) {
            pile.GetComponent<ScrapPile>().CollectPile();
        } else {
            pile.GetComponent<ScrapPile>().CollectPileWithTarget(customCollectTargetTransform);
        }
    }
}
