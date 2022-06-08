using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellAction : ModuleAction {

	public GameObject sellPrefab;

	private TrainBuilding _building;
	private ModuleHealth _health;

	protected override void _Start() {
		_building = GetComponent<TrainBuilding>();
		_health = GetComponent<ModuleHealth>();

		if (!_building || !_health) {
			this.enabled = false;
		}
	}

	protected override void _EngageAction() {
		GetComponentInParent<Slot>().RemoveBuilding(GetComponent<TrainBuilding>());
		Instantiate(sellPrefab, transform.position, transform.rotation);
		Destroy(gameObject);
	}

	protected override void _Update() {
			var hpPercent = _health.currentHealth / _health.maxHealth;
			cost = (int)(-_building.cost * 0.5 * hpPercent);
	}
}
