using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SellAction : ModuleAction, IActiveDuringCombat, IActiveDuringShopping {

	private TrainBuilding _building;
	private ModuleHealth _health;

	public bool isCombatMode = true;

	public bool isScrapPile = false;
	public bool isFuelPile = false;

	[NonSerialized]
	public UnityEvent sellEvent = new UnityEvent();
	protected override void _Start() {
		_building = GetComponent<TrainBuilding>();
		_health = GetComponent<ModuleHealth>();

		if (!_building || !_health) {
			this.enabled = false;
		}
	}

	protected override void _EngageAction() {
		Instantiate(DataHolder.s.sellPrefab, transform.position, transform.rotation);
		sellEvent?.Invoke();

		if (isScrapPile) {
			UpgradesController.s.RemoveScrapFromShopArea();
		}

		if (isFuelPile) {
			UpgradesController.s.RemoveFuelFromShopArea();
		}
		
		Train.s.SaveTrainState();
		Destroy(gameObject);
	}

	protected override void _Update() {
		/*var hpPercent = _health.currentHealth / _health.maxHealth;
		var moneyGivenPercent = (hpPercent * 0.5f) + 0.5f; // even at 0 hp give some of the cost back.
		var multiplier = 0.5f;
		if (!isCombatMode)
			multiplier = 0.75f;
		cost = (int)(-_building.cost * multiplier * moneyGivenPercent);*/
		cost = (int)-_building.cost;
	}
	
	public void ActivateForCombat() {
		this.enabled = true;
		isCombatMode = true;
	}

	public void ActivateForShopping() {
		this.enabled = true;
		isCombatMode = false;
	}

	public void Disable() {
		this.enabled = false;
	}
}
