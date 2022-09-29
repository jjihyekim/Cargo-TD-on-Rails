using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellAction : ModuleAction, IActiveDuringCombat, IActiveDuringShopping {

	private TrainBuilding _building;
	private ModuleHealth _health;

	public bool isCombatMode = true;

	protected override void _Start() {
		_building = GetComponent<TrainBuilding>();
		_health = GetComponent<ModuleHealth>();

		if (!_building || !_health) {
			this.enabled = false;
		}
	}

	protected override void _EngageAction() {
		GetComponentInParent<Slot>().RemoveBuilding(GetComponent<TrainBuilding>());
		Instantiate(DataHolder.s.sellPrefab, transform.position, transform.rotation);
		Destroy(gameObject);
	}

	protected override void _Update() {
		var hpPercent = _health.currentHealth / _health.maxHealth;
		var multiplier = 0.5f;
		if (!isCombatMode)
			multiplier = 0.75f;
		cost = (int)(-_building.cost * multiplier * hpPercent);
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
