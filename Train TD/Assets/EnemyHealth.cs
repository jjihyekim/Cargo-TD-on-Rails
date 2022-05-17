using System;
using System.Collections;
using System.Collections.Generic;
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
	public void DealDamage(float damage) {
		currentHealth -= damage;

		if (currentHealth <= 0 && isAlive) {
			Die();
		}
	}

	private void Start() {
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

		Instantiate(deathPrefab, pos, rot);

		Destroy(gameObject);
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
}
