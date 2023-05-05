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
    public bool isLeft;

    private LineRenderer lineRenderer;
    
    public Material deadlyMaterial;
    public Material safeMaterial;

    public float waveSpawnXSpread = 0;
    public float myXOffset = 0;
    public float targetXOffset = 0;


    private bool isTeleporting = false;
    private Vector2 teleportTiming = Vector2.zero;

    public bool isStealing = false;
    public bool isLeaving = false;

    public bool IsTrain() {
        return false;
    }
    
    public Sprite GetMainSprite() {
        return DataHolder.s.GetEnemy(myEnemy.enemyUniqueName).GetComponent<EnemySwarmMaker>().enemyIcon;
    }

    public Sprite GetGunSprite() {
        return DataHolder.s.GetEnemy(myEnemy.enemyUniqueName).GetComponent<EnemySwarmMaker>().GetGunSprite();
    }
    
    private void Start() {
        lineRenderer = GetComponentInChildren<LineRenderer>();
    }

    public void SetUp(EnemyIdentifier data, float position, bool isMoving, bool _isLeft, PowerUpScriptable powerUp) {
        myEnemy = data;
        var en = DataHolder.s.GetEnemy(myEnemy.enemyUniqueName);
        var mySwarm = en.GetComponent<EnemySwarmMaker>();
        if (mySwarm == null) {
            Debug.LogError($"Enemy is missing swarm maker {en.gameObject.name} {data.enemyUniqueName}");
        }

        bool hasPowerUp = data.enemyUniqueName == LevelReferences.s.powerUpSpawnerEnemy.enemyUniqueName;
       
        mySpeed = mySwarm.speed;
        isTeleporting = mySwarm.isTeleporting;
        teleportTiming = mySwarm.teleportTiming;
        isStealing = mySwarm.isStealing;
        wavePosition = position;
        isWaveMoving = isMoving;

        if (isMoving) {
            currentSpeed = mySpeed;
        }
        
        isLeft = _isLeft;
        SpawnEnemy(hasPowerUp, powerUp);
        
        SetTargetPosition();
        myXOffset = targetXOffset;

        DistanceAndEnemyRadarController.s.RegisterUnit(this);

        teleportTimer = 0;
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
    public float teleportTimer;

    private bool instantLerp = false;
    public void UpdateBasedOnDistance(float playerPos) {
        if (PlayStateMaster.s.isCombatInProgress()) {
            distance = Mathf.Abs(playerPos - wavePosition);
            
            if (!isWaveMoving) {
                if (distance < 10)
                    isWaveMoving = true;
            }

            targetSpeed = Mathf.Min(mySpeed, Mathf.Max(distance, LevelReferences.s.speed) + 0.2f);

            if (isWaveMoving) {
                if (playerPos < wavePosition && !isLeaving) {
                    targetSpeed = Mathf.Min(mySpeed,Mathf.FloorToInt(LevelReferences.s.speed - 1));
                }

                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeDelta * Time.deltaTime);

                wavePosition += currentSpeed * Time.deltaTime;
            }

            var targetPos = Vector3.forward * (wavePosition - playerPos + currentDistanceOffset - 0.2f) + Vector3.left * myXOffset;
            transform.position = Vector3.Lerp(transform.position,targetPos, 20*Time.deltaTime);
            if (instantLerp) {
                transform.position = targetPos;
                instantLerp = false;
            }
            var effectiveSpeed = currentSpeed - slowAmount;
            slowAmount -= slowDecay * Time.deltaTime;
            slowAmount = Mathf.Clamp(slowAmount, 0, float.MaxValue);
            if (slowAmount <= 0) {
                ToggleSlowedEffect(false);
            }
            
            currentDistanceOffset = Mathf.MoveTowards(currentDistanceOffset, targetDistanceOffset, effectiveSpeed / 2f * Time.deltaTime);

            targetDistChangeTimer -= Time.deltaTime;
            if (targetDistChangeTimer <= 0 && !isLeaving) {
                targetDistChangeTimer = Random.Range(2f, 15f);
                if (isStealing) {
                    targetDistChangeTimer = Random.Range(15f, 25f);
                }
                SetTargetPosition();
            }
            
            if (isTeleporting) {
                teleportTimer -= Time.deltaTime;
                if (teleportTimer <= 0 && distance < 10) {
                    teleportTimer = Random.Range(teleportTiming.x, teleportTiming.y);
                    Teleport();
                }
                
                if (distance > 30 && teleportTimer <= 0 && distance < 40) {
                    teleportTimer = Random.Range(teleportTiming.y * 0.9f, teleportTiming.y * 1.1f);
                    Teleport();
                }
            }

            var showRouteDisplay = distance > 10 && distance < 60;
            if (waveDisplay != null && !showRouteDisplay) {
                PlayEnemyEnterSound();
            }

            if (showRouteDisplay) {
                CreateRouteDisplay();
            } else {
                DestroyRouteDisplay();
            }
            
            if (distance > 100 && isLeaving) {
                Destroy(gameObject);
            }

            myXOffset = Mathf.MoveTowards(myXOffset, targetXOffset, 0.1f * Time.deltaTime * currentSpeed);
            
        } else if(PlayStateMaster.s.isCombatFinished()) {
            transform.position = Vector3.forward * (wavePosition - playerPos + currentDistanceOffset - 0.2f) + Vector3.left * myXOffset;
            wavePosition += currentSpeed * Time.deltaTime;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedChangeDelta * Time.deltaTime);
            targetSpeed = mySpeed;
            DestroyRouteDisplay();
        }
    }

    private List<GameObject> teleportEffects = new List<GameObject>();
    public void Teleport() {
        targetDistChangeTimer = Random.Range(4f, 15f);
        var cars = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < cars.Length; i++) {
            var target = cars[i].transform;
            var effect = Instantiate(LevelReferences.s.teleportStartEffect, target.transform.position,Quaternion.identity);
            effect.transform.SetParent(target);
            teleportEffects.Add(effect);
        }
        
        Invoke(nameof(FinishTeleport), LevelReferences.s.teleportTime);
    }

    void FinishTeleport() {
        for (int i = 0; i < teleportEffects.Count; i++) {
            if (teleportEffects[i] != null) {
                teleportEffects[i].transform.SetParent(null);
            }
        }

        teleportEffects.Clear();

        currentSpeed = LevelReferences.s.speed;
        isLeft = !isLeft;
        SetTargetPosition();
        myXOffset = targetXOffset;
        currentDistanceOffset = targetDistanceOffset;
        wavePosition = SpeedController.s.currentDistance;
        instantLerp = true;
        
        var cars = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < cars.Length; i++) {
            var target = cars[i].transform;
            Instantiate(LevelReferences.s.teleportCompleteEffect, target.transform.position, Quaternion.identity).transform.SetParent(target);
        }
    }

    private void SetTargetPosition() {
        if (!isStealing) {
            var trainLength = Train.s.GetTrainLength();
            var halfLength = (trainLength / 2f) + DataHolder.s.cartLength;
            targetDistanceOffset = Random.Range(-halfLength, halfLength);

            targetXOffset = Random.Range(0.7f+waveSpawnXSpread, 3.2f-waveSpawnXSpread);
            if (!isLeft)
                targetXOffset = -targetXOffset;
        } else {
            SetTargetPositionStealing();
        }
    }

    private ModuleStorage lastCargo;
    void SetTargetPositionStealing() {
        var cargos = Train.s.GetComponentsInChildren<ModuleStorage>();

        ModuleStorage randomCargo = null;
        for (int i = 0; i < 10; i++) {
            randomCargo = cargos[Random.Range(0, cargos.Length)];

            if(lastCargo == null)
                break;
            if (Vector3.Distance(randomCargo.transform.position, lastCargo.transform.position) > 0.01f) {
                break;
            }
        }

        if (randomCargo == null) {
            return;
        }

        targetDistanceOffset = randomCargo.transform.position.z + Random.Range(-0.5f, 0.5f); // this works because the Train is at perfectly 0

        targetXOffset = Random.Range(0.55f, 0.9f);
        
        GetComponentInChildren<StealerHarpoonModule>().SetTarget(randomCargo); // we are assuming stealers come in packs of 1
        
        if (!isLeft)
            targetXOffset = -targetXOffset;
    }
    void SpawnEnemy(bool hasPowerUp, PowerUpScriptable powerUp) {
        drawnEnemies = Instantiate(DataHolder.s.GetEnemy(myEnemy.enemyUniqueName), transform).GetComponent<EnemySwarmMaker>();
        drawnEnemies.transform.ResetTransformation();
        waveSpawnXSpread = drawnEnemies.SetData(myEnemy.enemyCount);

        if (hasPowerUp) {
            drawnEnemies.enemyIcon = powerUp.icon;
        }
    }

    void DestroyRouteDisplay() {
        if (waveDisplay != null) {
            Destroy(waveDisplay.gameObject);
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

    public bool isLeftUnit() {
        return isLeft;
    }

    public float GetSpeed() {
        return currentSpeed;
    }
    
    
    public float slowAmount;
    public float slowDecay = 0.1f;
    public void AddSlow(float amount) {
        slowAmount += amount;
        ToggleSlowedEffect(true);
    }
    
    public List<GameObject> activeSlowedEffects = new List<GameObject>();
    private bool isSlowedOn = false;
    void ToggleSlowedEffect(bool isOn) {
        if (isOn && !isSlowedOn) {
            var enemies = GetComponentsInChildren<EnemyHealth>();
            for (int i = 0; i < enemies.Length; i++) {
                var effect = Instantiate(LevelReferences.s.currentlySlowedEffect, enemies[i].transform.position, Quaternion.identity);
                effect.transform.SetParent(enemies[i].transform);
                activeSlowedEffects.Add(effect);
            }

            isSlowedOn = true;
        }

        if (!isOn && isSlowedOn) {
            for (int i = 0; i < activeSlowedEffects.Count; i++) {
                SmartDestroy(activeSlowedEffects[i].gameObject);
            }
            
            activeSlowedEffects.Clear();
            isSlowedOn = false;
        }
    }

    void SmartDestroy(GameObject target) {
        var particles = GetComponentsInChildren<ParticleSystem>();

        foreach (var particle in particles) {
            particle.transform.SetParent(null);
            particle.Stop();
            Destroy(particle.gameObject, 1f);
        }
            
        Destroy(target);
    }

    public void Leave() {
        isLeaving = true;
        isStealing = false;
        isTeleporting = false;
        isWaveMoving = true;
        
        SetTargetPosition();
        targetDistanceOffset = SpeedController.s.missionDistance;
    }
}
