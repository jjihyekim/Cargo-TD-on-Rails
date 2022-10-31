using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

public class EnemyWave : MonoBehaviour, IShowOnDistanceRadar, ISpeedForEngineSoundsProvider {
    public EnemyIdentifier myEnemy;
    public EnemySwarmMaker drawnEnemies;
    public float mySpeed;

    public MiniGUI_IncomingWave waveDisplay;
                
    public bool isWaveMoving = false;
    public float wavePosition = -1;

    private LineRenderer lineRenderer;
    
    public Material deadlyMaterial;
    public Material safeMaterial;

    public float myXOffset = 0;

    private void Start() {
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    public void SetUp(EnemyIdentifier data, float position, bool isMoving, bool isLeft) {
        myEnemy = data;
        mySpeed = DataHolder.s.GetEnemy(myEnemy.enemyUniqueName).GetComponent<EnemySwarmMaker>().speed;
        wavePosition = position;
        isWaveMoving = isMoving;
        SpawnEnemy();

        myXOffset = Random.Range(0.7f, 3.2f);
        if (isLeft)
            myXOffset = -myXOffset;
        
        DistanceAndEnemyRadarController.s.RegisterUnit(this);
    }

    private void OnDestroy() {
        DestroyRouteDisplay();
        DistanceAndEnemyRadarController.s.RemoveUnit(this);
        EnemyWavesController.s.RemoveWave(this);
    }


    public float speedChangeDelta = 0.5f;
    public float currentSpeed = 0;
    public float targetSpeed = 0;
    public float distance;
    public float targetDistanceOffset;
    public float currentDistanceOffset;
    public float targetDistChangeTimer;
    public void UpdateBasedOnDistance(float playerPos) {
        if (SceneLoader.s.isLevelInProgress) {
            distance = Mathf.Abs(playerPos - wavePosition);


            if (!isWaveMoving) {
                if (distance < 10)
                    isWaveMoving = true;
            }

            targetSpeed = Mathf.Min(mySpeed, Mathf.Max(distance, LevelReferences.s.speed) + 0.2f);

            if (isWaveMoving) {
                if (playerPos < wavePosition) {
                    targetSpeed = Mathf.FloorToInt(LevelReferences.s.speed - 1);
                }


                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeDelta * Time.deltaTime);

                wavePosition += currentSpeed * Time.deltaTime;
            }

            transform.position = Vector3.forward * (wavePosition - playerPos + currentDistanceOffset - 0.2f) + Vector3.left * myXOffset;
            currentDistanceOffset = Mathf.MoveTowards(currentDistanceOffset, targetDistanceOffset, currentSpeed / 2f * Time.deltaTime);

            targetDistChangeTimer -= Time.deltaTime;
            if (targetDistChangeTimer <= 0) {
                targetDistChangeTimer = Random.Range(2f, 15f);
                var trainLength = Train.s.GetTrainLength();
                targetDistanceOffset = Random.Range(-trainLength, trainLength);
            }


            if (distance > 10 && distance < 60) {
                CreateRouteDisplay();
            } else {
                DestroyRouteDisplay();
            }
        } else {
            
            transform.position = Vector3.forward * (wavePosition - playerPos + currentDistanceOffset - 0.2f) + Vector3.left * myXOffset;
            wavePosition += currentSpeed * Time.deltaTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeDelta * Time.deltaTime);
            targetSpeed = mySpeed;
        }
    }

    void SpawnEnemy() {
        drawnEnemies = Instantiate(DataHolder.s.GetEnemy(myEnemy.enemyUniqueName), transform).GetComponent<EnemySwarmMaker>();
        drawnEnemies.transform.ResetTransformation();
        drawnEnemies.SetData(myEnemy.enemyCount);
    }

    void DestroyRouteDisplay() {
        if (waveDisplay != null) {
            Destroy(waveDisplay.gameObject);
            PlayEnemyEnterSound();
            //lineRenderer.enabled = false;
        }
    }

    void PlayEnemyEnterSound() {
        drawnEnemies.PlayEnemyEnterSound();
    }


    const float lineDistance = 2f;
    private const float lineHeight = 0.5f;
    void CreateRouteDisplay() {
        if (waveDisplay == null) {
            waveDisplay = Instantiate(LevelReferences.s.waveDisplayPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_IncomingWave>();
            waveDisplay.SetUp(this);


            var points = new List<Vector3>();
            
            var myPos = transform.position;
            var close = myPos;
            close.z = Mathf.Clamp(close.z, -2, 2);
            var far = myPos;
            far.z = Mathf.Clamp(far.z, -15, 15);

            close.y = 0.5f;
            far.y = 0.5f;
            
            points.Add(far);
            points.Add(close);

            lineRenderer = GetComponentInChildren<LineRenderer>();
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
            var enemyType = DataHolder.s.GetEnemy(myEnemy.enemyUniqueName).GetComponent<EnemyTypeData>().myType;
            lineRenderer.material = enemyType == EnemyTypeData.EnemyType.Deadly ? deadlyMaterial : safeMaterial;
            targetAlpha = 0f;
            lineRenderer.material.SetFloat("alpha", targetAlpha);
            lineRenderer.enabled = true;
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
        LerpLineRenderedAlpha();
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

    public float GetSpeed() {
        return currentSpeed;
    }
}
