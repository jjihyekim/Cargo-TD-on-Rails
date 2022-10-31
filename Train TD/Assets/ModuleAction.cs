using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public abstract class ModuleAction : UnlockableEffect {
	public string actionName = "unknown Action";

	public DataSaver.RunResources.Types myType = DataSaver.RunResources.Types.scraps;
	
	[Tooltip("put negative for earn money")]
	public int cost = 25;
	[Tooltip("put -1 for no cooldown")]
	public float cooldown = 15f;

	public float curCooldown = 0f;

	[NonSerialized]
	public bool canEngage = true; // Use this to lock engagement of actions, eg stop engine boosts while engine stop is in progress

	public void EngageAction() {
		if (canEngage) {
			if (curCooldown <= 0) {
				if (cost > 0) {
					if (SceneLoader.s.isLevelInProgress) {
						if (MoneyController.s.scraps > cost) {
							MoneyController.s.SubtractScraps(cost);
							curCooldown = cooldown;
							_EngageAction();
						}
					} else {
						if (DataSaver.s.GetCurrentSave().currentRun.myResources.scraps > cost) {
							DataSaver.s.GetCurrentSave().currentRun.myResources.AddResource(-cost, myType);
							curCooldown = cooldown;
							_EngageAction();
							DataSaver.s.SaveActiveGame();
						}
					}
				} else {
					if (SceneLoader.s.isLevelInProgress) {
						MoneyController.s.AddScraps(-cost);
						curCooldown = cooldown;
						_EngageAction();
					} else {
						DataSaver.s.GetCurrentSave().currentRun.myResources.AddResource(-cost, myType);
						curCooldown = cooldown;
						_EngageAction();
						DataSaver.s.SaveActiveGame();
					}
				}
			}
		}
	}

	public void RefundAction() {
		var cost = -this.cost;
		
		if (cost > 0) {
			if (SceneLoader.s.isLevelInProgress) {
				if (MoneyController.s.scraps > cost) {
					MoneyController.s.SubtractScraps(cost);
					curCooldown = cooldown;
					_EngageAction();
				}
			} else {
				if (DataSaver.s.GetCurrentSave().currentRun.myResources.scraps > cost) {
					DataSaver.s.GetCurrentSave().currentRun.myResources.AddResource(-cost, myType);
					curCooldown = cooldown;
					_EngageAction();
					DataSaver.s.SaveActiveGame();
				}
			}
		} else {
			if (SceneLoader.s.isLevelInProgress) {
				MoneyController.s.AddScraps(-cost);
				curCooldown = cooldown;
				_EngageAction();
			} else {
				DataSaver.s.GetCurrentSave().currentRun.myResources.AddResource(-cost, myType);
				curCooldown = cooldown;
				_EngageAction();
				DataSaver.s.SaveActiveGame();
			}
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