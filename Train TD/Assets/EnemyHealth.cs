using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IHealth {
	public float maxHealth = 20f;
	public float currentHealth = 20f;

	public int moneyReward = 25;


	public GameObject deathPrefab;
	public Transform aliveObject;

	public static int enemySpawned;
	public static int enemyKilled;

	public bool isAlive = true;

	public Transform uiTransform;
	
	
	[ReadOnly]
	public MiniGUI_HealthBar healthBar;

	[Tooltip("Will reduce incoming damage by 50% if gun doesn't have armor penetration")]
	public bool isArmored = false;
	public void DealDamage(float damage) {
		currentHealth -= damage;

		if (currentHealth <= 0 && isAlive) {
			Die();
		}
	}

	private void Start() {
		healthBar = Instantiate(LevelReferences.s.partHealthPrefab, LevelReferences.s.uiDisplayParent).GetComponent<MiniGUI_HealthBar>();
		healthBar.SetUp(this);
		enemySpawned += 1;
	}

	private void Update() {
		if (SceneLoader.s.isLevelFinished()) {
			if(isAlive)
				Die();
		}
	}


	void Die() {
		enemyKilled += 1;
		isAlive = false;
		LevelReferences.s.SpawnMoneyAtLocation(moneyReward, aliveObject.transform.position);

		var pos = aliveObject.position;
		var rot = aliveObject.rotation;

		Destroy(aliveObject.gameObject);
		Destroy(healthBar.gameObject);
		
		Instantiate(deathPrefab, pos, rot);

		Destroy(gameObject);
	}
	
	private void OnDestroy() {
		if(healthBar != null)
			if(healthBar.gameObject != null)
				Destroy(healthBar.gameObject);
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

#if UNITY_EDITOR
	[MethodButton("Die")]
	[SerializeField] private bool editorFoldout;
#endif
}


public interface IHealth {
	public void DealDamage(float damage);
	public bool IsPlayer();
	public GameObject GetGameObject();
	public Collider GetMainCollider();
	public bool HasArmor();
	public float GetHealthPercent();
	public string GetHealthRatioString();
}
