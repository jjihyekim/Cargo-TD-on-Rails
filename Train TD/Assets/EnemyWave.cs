using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class EnemyWave : MonoBehaviour, IShowOnDistanceRadar {
    public EnemyIdentifier myEnemy;
    public GameObject drawnEnemies;

    public MiniGUI_IncomingWave waveDisplay;
                
    public bool isWaveMoving = false;
    public float wavePosition = -1;

    private LineRenderer lineRenderer;
    
    public Material deadlyMaterial;
    public Material safeMaterial;
    
    private void Start() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetUp(EnemyIdentifier data, float position, bool isMoving, bool isLeft) {
        myEnemy = data;
        wavePosition = position;
        isWaveMoving = isMoving;
        SpawnEnemy();
        CreateRouteDisplay();
        
        DistanceAndEnemyRadarController.s.RegisterUnit(this);
    }

    private void OnDestroy() {
        
        DistanceAndEnemyRadarController.s.RemoveUnit(this);
    }


    public float speedChangeDelta = 0.5f;
    public float currentSpeed = 0;
    public float targetSpeed = 0;
    public void UpdateBasedOnDistance(float playerPos) {
        var distance = Mathf.Abs(playerPos - wavePosition);
        if (distance > 10 && distance < 30) {
            CreateRouteDisplay();
        } else {
            DestroyRouteDisplay();
        }

        if (!isWaveMoving) {
            if (distance < 10)
                isWaveMoving = true;
        }

        targetSpeed = myEnemy.enemySpeed;

        if (isWaveMoving) {
            if (playerPos < wavePosition) {
                targetSpeed = Mathf.FloorToInt(LevelReferences.s.speed - 1) ;
            }


            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeDelta * Time.deltaTime);
            
            wavePosition += currentSpeed * Time.deltaTime;
        }
        
        transform.position = Vector3.forward* (wavePosition - playerPos);
    }

    void SpawnEnemy() {
        var enemy = Instantiate(DataHolder.s.GetEnemy(myEnemy.enemyUniqueName), transform);
        enemy.transform.ResetTransformation();
        enemy.GetComponent<IData>()?.SetData(myEnemy.enemyCount);
    }

    void DestroyRouteDisplay() {
        if (waveDisplay != null) {
            Destroy(waveDisplay.gameObject);
            //lineRenderer.enabled = false;
        }
    }


    const float lineDistance = 2f;
    private const float lineHeight = 0.5f;
    void CreateRouteDisplay() {
        if (waveDisplay == null) {
            waveDisplay = Instantiate(LevelReferences.s.waveDisplayPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_IncomingWave>();
            waveDisplay.SetUp(this);


            /*var points = new List<Vector3>();
            var segmentCount = myCircuit.Waypoints.Length;
            for (int i = 0; i < segmentCount; i++) {
                points.Add(myCircuit.Waypoints[i].position);
                var xDirection = myData.isLeft ? 1 : -1;
                points[points.Count - 1] = new Vector3(points[points.Count - 1].x * xDirection, lineHeight, points[points.Count - 1].z);
                points[points.Count - 1] = transform.InverseTransformPoint(points[points.Count - 1]);
            }

            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());*/
            var enemyType = DataHolder.s.GetEnemy(myEnemy.enemyUniqueName).GetComponent<EnemyTypeData>().myType;
            /*lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.material = enemyType == EnemyTypeData.EnemyType.Deadly ? deadlyMaterial : safeMaterial;
            targetAlpha = 0f;
            lineRenderer.material.SetFloat("alpha", targetAlpha);
            lineRenderer.enabled = true;*/
        }
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
        //LerpLineRenderedAlpha();
    }

    void LerpLineRenderedAlpha() {
        if(isLerp)
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, currentLerpSpeed * Time.deltaTime);
        else
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, currentLerpSpeed * Time.deltaTime);
        
        lineRenderer.material.SetFloat("alpha", currentAlpha);
    }

    public float GetDistance() {
        return wavePosition;
    }

    public Sprite GetIcon() {
        return DataHolder.s.GetEnemy(myEnemy.enemyUniqueName).GetComponent<EnemySwarmMaker>().enemyIcon;
    }
}
