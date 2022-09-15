using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipOnHoverOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	private float hoverOverTime = 0.8f;
	private float curTimer = 0;

	public Tooltip tooltip;

	public bool pointerIn = false;
	public bool displayingTooltip = false;
	public void OnPointerEnter(PointerEventData eventData) {
		pointerIn = true;
	}

	public void OnPointerExit(PointerEventData eventData) {
		pointerIn = false;
		curTimer = 0;
	}

	private void Update() {
		if (pointerIn) {
			if (!displayingTooltip) {
				curTimer += Time.deltaTime;

				if (curTimer >= hoverOverTime) {
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
