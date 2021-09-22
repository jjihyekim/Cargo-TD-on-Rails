using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class EnemyWave : MonoBehaviour {
    public EnemyWaveData myData;

	public WaypointCircuit myCircuit;

    public MiniGUI_IncomingWave waveDisplay;
                
    public bool isWaveStarted = false;

    private LineRenderer lineRenderer;
    
    public Material deadlyMaterial;
    public Material safeMaterial;
    
    private void Start() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetUp(WaypointCircuit circuit, EnemyWaveData data) {
        myCircuit = circuit;
        myData = data;
    }

    public void UpdateBasedOnDistance(float distance) {
        if (!isWaveStarted) {
            if (distance > myData.startDistance) {
                isWaveStarted = true;
                StartCoroutine(WaveProcess());
            }
        }
    }


    IEnumerator WaveProcess() {
        var headsUpTime = myData.headsUpTime;
        var spawnTime = myCircuit.spawnTime;
        
        if (headsUpTime > spawnTime) {
            DisplayRoute();
            yield return new WaitForSeconds(headsUpTime - spawnTime);
            SpawnEnemy();
            yield return new WaitForSeconds(spawnTime);
            HideDisplay();
        } else {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnTime - headsUpTime);
            DisplayRoute();
            yield return new WaitForSeconds(headsUpTime);
            HideDisplay();
        }
    }

    void SpawnEnemy() {
        var enemy = Instantiate(DataHolder.s.GetEnemy(myData.enemyUniqueName), Vector3.back * 100, Quaternion.identity);
        enemy.GetComponent<IData>()?.SetData(myData.enemyData);
        var enemyCircuitFollowAI = enemy.GetComponent<EnemyCircuitFollowAI>();
        enemyCircuitFollowAI.myPath = myCircuit;
        enemyCircuitFollowAI.isLeft = myData.isLeft;
        enemyCircuitFollowAI.SnapToCurDistance();
        
    }

    void HideDisplay() {
        Destroy(waveDisplay.gameObject);
        lineRenderer.enabled = false;
    }


    const float lineDistance = 2f;
    private const float lineHeight = 0.5f;
    void DisplayRoute() {
        waveDisplay = Instantiate(LevelReferences.s.waveDisplayPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_IncomingWave>();
        waveDisplay.SetUp(this);


        var points = new List<Vector3>();
        var segmentCount = myCircuit.Waypoints.Length;
        for (int i = 0; i < segmentCount; i++) {
            points.Add(myCircuit.Waypoints[i].position);
            var xDirection = myData.isLeft ? 1 : -1;
            points[points.Count - 1] = new Vector3(points[points.Count - 1].x * xDirection, lineHeight, points[points.Count - 1].z);
            points[points.Count - 1] = transform.InverseTransformPoint(points[points.Count - 1]);
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        var enemyType = DataHolder.s.GetEnemy(myData.enemyUniqueName).GetComponent<EnemyTypeData>().myType;
        lineRenderer.material = enemyType == EnemyTypeData.EnemyType.Deadly ? deadlyMaterial : safeMaterial;
        targetAlpha = 0f;
        lineRenderer.material.SetFloat("alpha", targetAlpha);
        lineRenderer.enabled = true;
    }

    private float targetAlpha = 0;
    private float currentAlpha = 0;
    private float currentLerpSpeed = 2f;
    
    [Header("line alpha lerp options")]
    public float activeAlpha = 0.8f;
    public float disabledAlpha = 0.2f;
    public float onLerpSpeed = 2f;
    public float offLerpSpeed = 0.5f;

    public bool isLerp = true;

    public void ShowPath() {
        //lineRenderer.enabled = true;
        targetAlpha = activeAlpha;
        currentLerpSpeed = onLerpSpeed;
    }

    public void HidePath() {
        //lineRenderer.enabled = false;
        targetAlpha = disabledAlpha;
        currentLerpSpeed = offLerpSpeed;
    }


    private void Update() {
        LerpLineRenderedAlpha();
    }

    void LerpLineRenderedAlpha() {
        if(isLerp)
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, currentLerpSpeed * Time.deltaTime);
        else
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, currentLerpSpeed * Time.deltaTime);
        
        lineRenderer.material.SetFloat("alpha", currentAlpha);
    }
}
