using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellAction : ModuleAction {

	public GameObject sellPrefab;
	protected override void _EngageAction() {
		GetComponentInParent<Slot>().RemoveBuilding(GetComponent<TrainBuilding>());
		Instantiate(sellPrefab, transform.position, transform.rotation);
		Destroy(gameObject);
	}
}
