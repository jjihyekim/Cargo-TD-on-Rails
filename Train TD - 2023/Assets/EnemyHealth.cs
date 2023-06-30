using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class EnemyHealth : MonoBehaviour, IHealth {

	public float baseHealth = 200f;
	
	[ReadOnly]
	public float maxHealth = 20f;
	public float currentHealth = 20f;

	public int scrapReward = 10;
	public int ammoReward = 10;
	public int fuelReward = 10;


	public GameObject deathPrefab;
	public Transform aliveObject;

	public static int enemySpawned;
	public static int enemyKilled;

	public bool isAlive = true;

	public Transform uiTransform;
	[SerializeField] Transform cartRewardTransform;

	public static UnityEvent<bool> winSelfDestruct = new UnityEvent<bool>();
	
	[ReadOnly]
	public MiniGUI_HealthBar healthBar;

	[Tooltip("Will reduce incoming damage by 50% if gun doesn't have armor penetration")]
	public bool isArmored = false;

	public bool isComponentEnemy = false;

	public void DealDamage(float damage) {
		currentHealth -= damage;

		if (currentHealth <= 0 && isAlive) {
			Die();
		}
		
		SetBuildingShaderHealth(currentHealth / maxHealth);
	}

	public void Repair(float heal) {
		currentHealth += heal;

		if (currentHealth > maxHealth) {
			currentHealth = maxHealth;
		}

		SetBuildingShaderHealth(currentHealth / maxHealth);
	}

	void SetBuildingShaderHealth(float value) {
		var _renderers = GetComponentsInChildren<MeshRenderer>();
		for (int j = 0; j < _renderers.Length; j++) {
			var rend = _renderers[j];
			rend.material.SetFloat("_Health", value);
		}
	}
	
	void SetBuildingShaderBurn(float value) {
		var _renderers = GetComponentsInChildren<MeshRenderer>();
		value = value.Remap(0, 10, 0, 0.5f);
		value = Mathf.Clamp(value, 0, 2f);
		for (int j = 0; j < _renderers.Length; j++) {
			var rend = _renderers[j];
			rend.material.SetFloat("_Burn", value);
		}
	}


	float burnReduction = 0.5f;
	public float currentBurn = 0;
	public float burnSpeed = 0;
	private float lastBurn;
	public void BurnDamage(float damage) {
		burnSpeed += damage;
	}
	private void Update() {
		var burnDistance = Mathf.Max(burnSpeed / 2f, 1f);
		if (currentBurn >= burnDistance) {
			Instantiate(LevelReferences.s.damageNumbersPrefab, LevelReferences.s.uiDisplayParent)
				.GetComponent<MiniGUI_DamageNumber>()
				.SetUp(uiTransform, burnDistance, false, isArmored, true);
			DealDamage(burnDistance);

			currentBurn -= burnDistance;
		}

		if (burnSpeed > 0.05f) {
			currentBurn += burnSpeed * Time.deltaTime;
		}

		burnSpeed = Mathf.Lerp(burnSpeed,0,burnReduction*Time.deltaTime);

		if (Mathf.Abs(lastBurn - burnSpeed) > 1 || (lastBurn > 0 && burnSpeed <= 0)) {
			SetBuildingShaderBurn(burnSpeed);
			lastBurn = burnSpeed;
		}
	}

	public void SetUp() {
		healthBar = Instantiate(LevelReferences.s.enemyHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
		healthBar.SetUp(this);
		enemySpawned += 1;

		maxHealth *= 1 + WorldDifficultyController.s.currentHealthIncrease;
		maxHealth = baseHealth;
		currentHealth = maxHealth;
	}

	private void OnEnable() {
		winSelfDestruct.AddListener(Die);
	}

	private void OnDisable() {
		winSelfDestruct.RemoveListener(Die);
	}


	public bool rewardArtifactOnDeath = false;
	public string artifactRewardUniqueName;
	public Transform bonusArtifactUIStar;
	[Button]
	void Die(bool giveRewards = true) {
		enemyKilled += 1;
		isAlive = false;

		var extraRewards = GetComponentsInChildren<EnemyReward>();
		//var otherRewards = GetComponentInChildren<EnemyCartReward>();

		for (int i = 0; i < extraRewards.Length; i++) {
			switch (extraRewards[i].type) {
				case ResourceTypes.scraps:
					scrapReward += extraRewards[i].amount;
					break;
			}
		}
		
		if (giveRewards) {
			LevelReferences.s.SpawnResourceAtLocation(ResourceTypes.scraps, 
				scrapReward*TweakablesMaster.s.myTweakables.scrapEnemyRewardMultiplier, 
				aliveObject.transform.position);

			/*if (rewardPowerUp) {
				//PlayerActionsController.s.GetPowerUp(EnemyWavesController.s.powerUpScriptables.Dequeue());
			}*/

			/*if (otherRewards != null) {
				otherRewards.RewardPlayerCart();
			}*/

			if (rewardArtifactOnDeath) {
				ArtifactsController.s.GetBonusArtifact(bonusArtifactUIStar, artifactRewardUniqueName);
				bonusArtifactUIStar = null;
			}
		}

		var pos = aliveObject.position;
		var rot = aliveObject.rotation;

		Destroy(aliveObject.gameObject);
		Destroy(healthBar.gameObject);
		
		if(deathPrefab != null)
			Instantiate(deathPrefab, pos, rot);
		
		if(!isComponentEnemy)
			GetComponentInParent<EnemySwarmMaker>().EnemyDeath();

		Destroy(gameObject);
	}
	
	private void OnDestroy() {
		if(healthBar != null)
			if(healthBar.gameObject != null)
				Destroy(healthBar.gameObject);

		if (bonusArtifactUIStar != null) {
			Destroy(bonusArtifactUIStar.gameObject);
		}
	}

	public bool IsPlayer() {
		return false;
	}

	public GameObject GetGameObject() {
		return gameObject;
	}

	public Collider GetMainCollider() {
		return GetComponentInChildren<BoxCollider>();
	}

	public bool HasArmor() {
		return isArmored;
	}

	public float GetHealthPercent() {
		return currentHealth / maxHealth;
	}

	public string GetHealthRatioString() {
		return $"{currentHealth}/{maxHealth}";
	}

	public Transform GetUITransform() {
		return uiTransform;
	}

	public Transform GetCartRewardTransform() {
		if (cartRewardTransform != null) {
			return cartRewardTransform;
		} else {
			cartRewardTransform = new GameObject("cart reward transform").transform;
			cartRewardTransform.SetParent(transform.GetChild(0));
			cartRewardTransform.localPosition = Vector3.back*0.5f + Vector3.down*0.25f;
			return cartRewardTransform;
		}
	}

	[ReadOnly]
	public List<Outline> _outlines = new List<Outline>();

	void SetUpOutlines() {
		if (_outlines.Count == 0) {
			var renderers = GetComponentsInChildren<MeshRenderer>(true);

			foreach (var rend in renderers) {
				if (rend.GetComponent<Outline>() == null) {
					var outline = rend.gameObject.AddComponent<Outline>();
					outline.OutlineMode = Outline.Mode.OutlineAndSilhouette;
					outline.OutlineWidth = 5;
					outline.OutlineColor = new Color(0.0f, 0.833f, 1.0f, 1.0f);
					outline.enabled = false;
					_outlines.Add(outline);
				}
			}
		}
	}

	public void SetHighlightState(bool isHighlighted) {
		if (_outlines.Count == 0) {
			SetUpOutlines();
		}
        
		foreach (var outline in _outlines) {
			if (outline != null) {
				outline.enabled = isHighlighted;
			}
		}
	}
}


public interface IHealth {
	public void DealDamage(float damage);
	public void Repair(float heal);
	public void BurnDamage(float damage);
	public bool IsPlayer();
	public GameObject GetGameObject();
	public Collider GetMainCollider();
	public bool HasArmor();
	public float GetHealthPercent();
	public string GetHealthRatioString();

	public Transform GetUITransform();
}
