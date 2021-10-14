using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public abstract class ModuleAction : UnlockableEffect {
	public string actionName = "unknown Action";
	[Tooltip("put negative for earn money")]
	public int cost = 25;
	[Tooltip("put -1 for no cooldown")]
	public float cooldown = 15f;

	public float curCooldown = 0f;


	public void EngageAction() {
		if (curCooldown <= 0) {
			if (cost > 0) {
				if (MoneyController.s.money > cost) {
					MoneyController.s.SubtractMoney(cost);
					curCooldown = cooldown;
					_EngageAction();
				}
			} else {
				MoneyController.s.AddMoney(-cost);
				curCooldown = cooldown;
				_EngageAction();
			}
		}
	}

	protected abstract void _EngageAction();


	private void Update() {
		if (curCooldown > 0) {
			curCooldown -= Time.deltaTime;
			if (curCooldown < 0)
				curCooldown = 0;
		}
	}
}