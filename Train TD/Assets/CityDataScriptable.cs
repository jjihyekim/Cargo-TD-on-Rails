using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CityDataScriptable : ScriptableObject {
	public Sprite sprite;
	public GameObject worldMapCastle;
	public CityData cityData;
}


[Serializable]
public class CityData {
	public string nameSuffix;
	
	public CargoModule.CargoTypes[] cargosSold;

	public SupplyPrice[] prices;

	public bool canBuyCart = false;
}

[Serializable]
public class SupplyPrice {
	public ResourceTypes type;
	public int basePrice = 25;
	public float variance = 0.2f;
	public float priceIncrease = 1.2f;

	public SupplyPrice Copy() {
		var copy = new SupplyPrice() {
			type = type,
			basePrice = basePrice,
			variance = variance,
			priceIncrease = priceIncrease
		};
		return copy;
	}
}
