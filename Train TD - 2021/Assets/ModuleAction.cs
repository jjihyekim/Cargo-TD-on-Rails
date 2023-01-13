using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public abstract class ModuleAction : UnlockableEffect {
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

	public void EngageAction() {
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
				}
			}
		}
	}

	public void RefundAction() {
		var cost = -this.cost;

		if (cost > 0) {
			if (MoneyController.s.HasResource(myType, cost)) {
				MoneyController.s.ModifyResource(myType, -cost);
				curCooldown = cooldown;
				_EngageAction();
			}
		} else {
			MoneyController.s.ModifyResource(myType, -cost);
			curCooldown = cooldown;
			_EngageAction();
		}
	}

	protected abstract void _EngageAction();
	protected virtual void _Update(){}

	private void Update() {
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
}

public abstract class ModuleActionTweakable : ModuleAction {
	[Space]
	public float actionTime = 8;
	public float initialDelay = 1;
	public float endDelay = 2;
	public float boost = 5;
}