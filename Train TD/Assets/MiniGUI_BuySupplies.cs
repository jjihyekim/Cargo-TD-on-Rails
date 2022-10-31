using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MiniGUI_BuySupplies : MonoBehaviour
{
	public DataSaver.RunResources.Types myType;
	private SupplyPrice myPriceIndex;
	public int rewardAmount = 25;


	public Button myButton;
	public TMP_Text costText;

	public void Buy() {
		curRun.myResources.money -= myPriceIndex.basePrice;
		curRun.myResources.AddResource(rewardAmount, myType);
		myPriceIndex.basePrice = (int)(myPriceIndex.basePrice * myPriceIndex.priceIncrease);
		costText.text = myPriceIndex.basePrice.ToString();
		DataSaver.s.SaveActiveGame();
	}


	private DataSaver.RunState curRun;


	public void Setup() {
		curRun = DataSaver.s.GetCurrentSave().currentRun;
		if (curRun.shopInitialized) {
			myPriceIndex = null;

			for (int i = 0; i < curRun.currentShopPrices.Count; i++) {
				if (curRun.currentShopPrices[i].type == myType) {
					myPriceIndex = curRun.currentShopPrices[i];
				}
			}

		} else {
			var playerStar = curRun.map.GetPlayerStar();

			myPriceIndex = null;

			for (int i = 0; i < playerStar.city.prices.Length; i++) {
				if (playerStar.city.prices[i].type == myType) {
					myPriceIndex = playerStar.city.prices[i].Copy();
					myPriceIndex.basePrice = (int)(myPriceIndex.basePrice * (1 + Random.Range(-myPriceIndex.variance, myPriceIndex.variance)));
					break;
				}
			}
			
			curRun.currentShopPrices.Add(myPriceIndex);
		}
		
		if (myPriceIndex == null) {
			Destroy(gameObject);
			return;
		}

		costText.text = myPriceIndex.basePrice.ToString();
	}

	private void Update() {
		if (DataSaver.s.GetCurrentSave().isInARun && myPriceIndex != null) {
			myButton.interactable = curRun.myResources.money >= myPriceIndex.basePrice;
		}
	}
}
