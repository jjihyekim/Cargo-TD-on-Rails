using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipOnHoverOver : MonoBehaviour {

	private float curTimer = 0;

	public Tooltip tooltip;

	public bool pointerIn = false;
	public bool displayingTooltip = false;
	public void OnPointerEnter() {
		pointerIn = true;
	}

	public void OnPointerExit() {
		pointerIn = false;
		curTimer = 0;
	}

	private void Update() {
		if (pointerIn) {
			if (!displayingTooltip) {
				curTimer += Time.deltaTime;

				if (curTimer >= TooltipsMaster.tooltipShowTime) {
					ShowTooltip();
				}
			}
		} else {
			HideTooltip();
		}
	}


	void ShowTooltip() {
		if (!displayingTooltip) {
			displayingTooltip = true;
			TooltipsMaster.s.ShowTooltip(tooltip);
		}
	}

	void HideTooltip() {
		if (displayingTooltip) {
			displayingTooltip = false;
			TooltipsMaster.s.HideTooltip();
		}
	}
	
}
