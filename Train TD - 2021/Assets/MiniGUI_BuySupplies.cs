using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MiniGUI_BuySupplies : MonoBehaviour
{
	public ResourceTypes myType;
	private SupplyPrice myPriceIndex;
	public int rewardAmount = 25;


	public Button myButton;
	public TMP_Text costText;

	public void Buy() {
		MoneyController.s.ModifyResource(ResourceTypes.money, -myPriceIndex.basePrice);
		MoneyController.s.ModifyResource(myType, rewardAmount);
		myPriceIndex.basePrice = (int)(myPriceIndex.basePrice * myPriceIndex.priceIncrease);
		costText.text = myPriceIndex.basePrice.ToString();
		DataSaver.s.SaveActiveGame();
	}


	private DataSaver.RunState curRun;


	public void Setup() {
		curRun = DataSaver.s.GetCurrentSave().currentRun;
		if (curRun.shopInitialized) {
			myPriceIndex = null;

			/*for (int i = 0; i < curRun.currentShopPrices.Count; i++) {
				if (curRun.currentShopPrices[i].type == myType) {
					myPriceIndex = curRun.currentShopPrices[i];
					break;
				}
			}*/

		} 

		if (myPriceIndex == null) {
			gameObject.SetActive( false);
			return;
		} 
			
		gameObject.SetActive( true);

		costText.text = myPriceIndex.basePrice.ToString();
	}

	private void Update() {
		if (DataSaver.s.GetCurrentSave().isInARun && myPriceIndex != null) {
			myButton.interactable = MoneyController.s.HasResource(ResourceTypes.money, myPriceIndex.basePrice);
		}
	}
}
