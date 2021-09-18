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
    public GameObject rocketExplosionEffectPrefab;
    public GameObject waveDisplayPrefab;
    public GameObject partHealthPrefab;
    public Transform uiDisplayParent;

    public float healthBarPositionOffset = 0.1f;

    public float speed = 1f;

    public static List<PossibleTarget> allTargets = new List<PossibleTarget>();

    public GameObject scrapPile;

    public static List<ScrapPile> allScraps = new List<ScrapPile>();
    public Cart[] carts = new Cart[0];

    
    public Material hologramBuildable;
    public Material hologramCantBuild;
    
    private void Awake() {
        s = this;
    }


    const int maxMoneyPileCount = 25;
    public void SpawnMoneyAtLocation(int amount, Vector3 location) {
        int count = Mathf.CeilToInt(amount / 25);

        while(amount > 0) {
            var random = Random.insideUnitCircle * (count / 4f);
            var pile = Instantiate(scrapPile, location + new Vector3(random.x, 0, random.y), Quaternion.identity);
            var pileComp = pile.GetComponent<ScrapPile>();
            var targetAmount = amount % maxMoneyPileCount;
            if (targetAmount == 0)
                targetAmount += maxMoneyPileCount;
            pileComp.SetUp(targetAmount);
            allScraps.Add(pileComp);
            amount -= 25;
        }
    }
}
