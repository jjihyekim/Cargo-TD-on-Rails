using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniGUI_BuySupplies : MonoBehaviour
{
	public enum Types {
		fuel, scraps
	}


	public Types myType;
	public int moneyCost = 25;
	public int rewardAmount = 25;


	public Button myButton;
	//public TMP_Text costText;

	public void Buy() {
		curRun.myResources.money -= moneyCost;
		switch (myType) {
			case Types.fuel:
				curRun.myResources.fuel += rewardAmount;
				break;
			case Types.scraps:
				curRun.myResources.scraps += rewardAmount;
				break;
		}
		
		DataSaver.s.SaveActiveGame();
	}


	private DataSaver.RunState curRun => DataSaver.s.GetCurrentSave().currentRun;


	private void Update() {
		myButton.interactable = curRun.myResources.money >= moneyCost;
	}
}
