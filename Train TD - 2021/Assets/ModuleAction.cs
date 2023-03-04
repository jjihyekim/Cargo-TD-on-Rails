using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public abstract class ModuleAction : MonoBehaviour {
	public string actionName = "unknown Action";

	public ResourceTypes myType = ResourceTypes.scraps;
	
	[Tooltip("put negative for earn money")]
	public int cost = 25;
	[Tooltip("put -1 for no cooldown")]
	public float cooldown = 15f;

	public float curCooldown = 0f;

	public bool canEngage = true; // Use this to lock engagement of actions, eg stop engine boosts while engine stop is in progress
	public bool canAfford = true;
	public bool isCooldownOver = true;

	public AudioClip[] soundEffect;

	public Tooltip myTooltip;
	protected virtual void _Start() { }
	public void Start() { _Start();}

	public void EngageForFree() {
		_EngageAction();
		
		if (!SceneLoader.s.isLevelInProgress)
			Train.s.SaveTrainState();

		if (soundEffect.Length > 0) {
			SoundscapeController.s.PlayModuleSkillActivate(soundEffect[Random.Range(0, soundEffect.Length)]);
		}
	}

	public bool EngageAction() {
		if (canEngage) {
			if (curCooldown <= 0) {
				if (cost > 0) {
					if (MoneyController.s.HasResource(myType, cost)) {
						MoneyController.s.ModifyResource(myType, -cost);
						curCooldown = cooldown;
						_EngageAction();
						
						if (!SceneLoader.s.isLevelInProgress)
							Train.s.SaveTrainState();

						if (soundEffect.Length > 0) {
							SoundscapeController.s.PlayModuleSkillActivate(soundEffect[Random.Range(0, soundEffect.Length)]);
						}

						return true;
					}
				} else {
					//MoneyController.s.ModifyResource(myType, -cost);
					LevelReferences.s.SpawnResourceAtLocation(myType, -cost, transform.position);
					curCooldown = cooldown;
					_EngageAction();
					
					if (!SceneLoader.s.isLevelInProgress)
						Train.s.SaveTrainState();
					
					if (soundEffect.Length > 0) {
						SoundscapeController.s.PlayModuleSkillActivate(soundEffect[Random.Range(0, soundEffect.Length)]);
					}

					return true;
				}
			}
		}

		return false;
	}

	public void RefundAction() {
		var refundAmount = -cost;

		if (refundAmount > 0) {
			if (MoneyController.s.HasResource(myType, refundAmount)) {
				MoneyController.s.ModifyResource(myType, -refundAmount);
				curCooldown = cooldown;
				_EngageAction();
			}
		} else {
			MoneyController.s.ModifyResource(myType, -refundAmount);
			curCooldown = cooldown;
			_EngageAction();
		}
	}

	protected abstract void _EngageAction();
	protected virtual void _Update(){}

	public void Update() {
		if (curCooldown > 0) {
			curCooldown -= Time.deltaTime;
			if (curCooldown < 0)
				curCooldown = 0;
		}

		isCooldownOver = curCooldown <= 0;

		if (cost > 0) {
			canAfford = MoneyController.s.HasResource(myType, cost);
		}
		
		_Update();
	}
	
	protected void SetBoostStatus(bool isBoosting) {
		var boostVal = isBoosting ? 2.5f : 0f;
		var renderers = GetComponentsInChildren<MeshRenderer>();
		for (int j = 0; j < renderers.Length; j++) {
			var rend = renderers[j];
			rend.material.SetFloat("_Boost_Amount", boostVal);
		}
	}

	public Tooltip GetTooltip() {
		return myTooltip;
	}
}

public abstract class ModuleActionTweakable : ModuleAction {
	[Space]
	public float actionTime = 8;
	public float initialDelay = 1;
	public float endDelay = 2;
	public float boost = 5;
}