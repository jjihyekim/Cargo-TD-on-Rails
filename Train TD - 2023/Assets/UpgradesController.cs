using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UpgradesController : MonoBehaviour {
	public static UpgradesController s;
	private void Awake() {
		s = this;
	}
	
	
	public enum CartRarity {
		common, rare, epic
	}

	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier1Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> firstShopCarts = new List<string>();
	/*[ValueDropdown("GetAllModuleNames")]
	public List<string> tier2Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier3Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> bossBuildings = new List<string>();*/

	public SnapCartLocation[] fleaMarketLocations;
	public Transform shopableComponentsParent;
	public GameObject buildingCargo;

	public SnapCartLocation leftCargoParent;
	public SnapCartLocation rightCargoParent;

	public Transform leftRewardPos;
	public Transform rightRewardPos;

	public List<Cart> shopCarts;
	public Cart leftCargo;
	public Cart rightCargo;

	public void RemoveCartFromShop(Cart cart) {
		shopCarts.Remove(cart);
		SaveShopCartState();
	}

	public void AddCartToShop(Cart cart, CartLocation location, bool doSave = true) {
		cart.myLocation = location;
		shopCarts.Add(cart);
		if(doSave)
			SaveShopCartState();
	}

	public void ChangeCartLocation(Cart cart, CartLocation location) {
		cart.myLocation = location;
		SaveShopCartState();
	}

	public void SaveCartStateWithDelay() {
		CancelInvoke(nameof(SaveShopCartState));
		Invoke(nameof(SaveShopCartState), 2f);
	}

	public SnapCartLocation[] cargoSwapLocations;
	
	public void UpdateCargoHighlights() {
		var cargoCount = CheckIfCanGo();

		if (cargoCount > 0) {
			for (int i = 0; i < cargoSwapLocations.Length; i++) {
				cargoSwapLocations[i].SetEmptyStatus(cargoSwapLocations[i].IsEmpty());
			}
		} else {
			for (int i = 0; i < cargoSwapLocations.Length; i++) {
				cargoSwapLocations[i].SetEmptyStatus(false);
			}
		}
	}

	public void UpdateCartShopHighlights() {
		for (int i = 0; i < fleaMarketLocations.Length; i++) {
			fleaMarketLocations[i].SetEmptyStatus(fleaMarketLocations[i].IsEmpty());
			//_cargoLocations[i].SetEmptyStatus(isLocationEmpty);
		}
	}
	
	public void SnapDestinationCargos(Cart lastCart) {
		var directionSet = false;
		if (leftCargo.myLocation == CartLocation.train && rightCargo.myLocation == CartLocation.train) {
			if (leftCargo == lastCart) {
				PutCargoBackInDestinationSelect(rightCargo, false);
			}

			if (rightCargo == lastCart) {
				PutCargoBackInDestinationSelect(leftCargo, true);
			}
		}

		if (leftCargo.myLocation == CartLocation.train && rightCargo.myLocation != CartLocation.train) {
			directionSet = true;
			ShopStateController.s.SetGoingLeft();

			PutCargoBackInDestinationSelect(rightCargo, false);
		}
		
		if (rightCargo.myLocation == CartLocation.train && leftCargo.myLocation != CartLocation.train) {
			directionSet = true;
			ShopStateController.s.SetGoingRight();

			PutCargoBackInDestinationSelect(leftCargo, true);
		}

		if (!directionSet) {
			PutCargoBackInDestinationSelect(leftCargo, true);
			PutCargoBackInDestinationSelect(rightCargo, false);
		}
		
		CheckIfCanGo();
	}

	void PutCargoBackInDestinationSelect(Cart cart, bool isLeft) {
		var targetParent = isLeft ? leftCargoParent.snapTransform : rightCargoParent.snapTransform;

		if (cart.myLocation == CartLocation.train) {
			Train.s.RemoveCart(cart);
			shopCarts.Add(cart);
		}
		
		cart.myLocation = CartLocation.destinationSelect;
		cart.transform.SetParent(targetParent);

		var shopState = DataSaver.s.GetCurrentSave().currentRun.shopState;
		var cargo = cart.GetComponentInChildren<CargoModule>();
		var cargoState = new DataSaver.TrainState.CartState.CargoState() {
			cargoReward = cargo.GetReward(),
			isLeftCargo = cargo.isLeftCargo
		};
		
		if (isLeft) {
			shopState.leftCargo = cargoState;
		} else {
			shopState.rightCargo = cargoState;
		}

		cart.GetComponent<Rigidbody>().isKinematic = true;
		cart.GetComponent<Rigidbody>().useGravity = false;

		SaveShopCartState();
		Train.s.SaveTrainState();
	}


	public float destinationCargoLerpSpeed = 5f;
	public float destinationCargoSlerpSpeed = 20f;
	private void Update() {
		if (rightCargo != null) {
			if (rightCargo.myLocation == CartLocation.destinationSelect) {
				rightCargo.transform.localPosition = Vector3.Lerp(rightCargo.transform.localPosition, Vector3.zero, destinationCargoLerpSpeed * Time.deltaTime);
				rightCargo.transform.localRotation = Quaternion.Slerp(rightCargo.transform.localRotation, Quaternion.identity, destinationCargoSlerpSpeed * Time.deltaTime);
			}
		}

		if (leftCargo != null) {
			if (leftCargo.myLocation == CartLocation.destinationSelect) {
				leftCargo.transform.localPosition = Vector3.Lerp(leftCargo.transform.localPosition, Vector3.zero, destinationCargoLerpSpeed * Time.deltaTime);
				leftCargo.transform.localRotation = Quaternion.Slerp(leftCargo.transform.localRotation, Quaternion.identity, destinationCargoSlerpSpeed * Time.deltaTime);
			}
		}
	}

	public void SaveShopCartState() {
		var shopState = new ShopState();
		for (int i = 0; i < shopCarts.Count; i++) {
			var cart = shopCarts[i];
			if (cart.isCargo && PlayStateMaster.s.isShop()) { // if we aren't in the shop cargos are also regular carts
				var cargo = cart.GetComponentInChildren<CargoModule>();
				if (cargo.isLeftCargo) { 
					shopState.leftCargo = new DataSaver.TrainState.CartState.CargoState() {
						cargoReward = cargo.GetReward(),
						isLeftCargo = cargo.isLeftCargo
					};
				} else { 
					shopState.rightCargo = new DataSaver.TrainState.CartState.CargoState() {
						cargoReward = cargo.GetReward(),
						isLeftCargo = cargo.isLeftCargo
					};
				}
			} else {
				shopState.cartStates.Add(new WorldCartState() {
					location = cart.myLocation,
					pos = cart.transform.position,
					rot = cart.transform.rotation,
					state = Train.GetStateFromCart(cart)
				});
			}
		}

		DataSaver.s.GetCurrentSave().currentRun.shopState = shopState;
		DataSaver.s.SaveActiveGame();
	}
	

	[System.Serializable]
	public class ShopState {
		public List<WorldCartState> cartStates = new List<WorldCartState>();
		public DataSaver.TrainState.CartState.CargoState leftCargo;
		public DataSaver.TrainState.CartState.CargoState rightCargo;
	}
	
	[Serializable]
	public class WorldCartState {
		public CartLocation location;
		public Vector3 pos;
		public Quaternion rot;
		public DataSaver.TrainState.CartState state = new DataSaver.TrainState.CartState();
	}

	[Serializable]
	public enum CartLocation {
		train = 0, market = 1, world = 2, forge = 3, destinationSelect = 4, cargoDelivery = 5
	}
	
	void InitializeShop(DataSaver.RunState state) {
		state.shopState = new ShopState();
		var buildingCargoCount = 3; 

		for (int i = 0; i < buildingCargoCount; i++) {
			state.shopState.cartStates.Add(new WorldCartState() {
				location =  CartLocation.market,
				state = new DataSaver.TrainState.CartState() {
					uniqueName = GetRandomBuildingCargoForFleaMarket()
				}
			});
		}

		state.shopState.leftCargo = new DataSaver.TrainState.CartState.CargoState() {
			cargoReward = GetRandomBuildingCargoForDestinationReward(),
			isLeftCargo = true
		};
		state.shopState.rightCargo = new DataSaver.TrainState.CartState.CargoState() {
			cargoReward = GetRandomBuildingCargoForDestinationReward(),
			isLeftCargo = false
		};

		state.shopInitialized = true;
		DataSaver.s.SaveActiveGame();
	}

	public void DrawShopOptions() {
		var currentRun = DataSaver.s.GetCurrentSave().currentRun;
		if (!currentRun.shopInitialized) {
			InitializeShop(currentRun);
		}
		
		SpawnShopItems();
	}

	public void ClearCurrentShop() {
		transform.DeleteAllChildren();
		for (int i = shopCarts.Count-1; i >= 0; i--) {
			if(shopCarts[i] != null && shopCarts[i].gameObject != null)
				Destroy(shopCarts[i].gameObject);
		}
		shopCarts.Clear();
		
		var currentRun = DataSaver.s.GetCurrentSave().currentRun;
		currentRun.shopState = new ShopState();
		DataSaver.s.SaveActiveGame();
	}
	
	void SpawnShopItems() {
		transform.DeleteAllChildren();
		for (int i = shopCarts.Count-1; i >= 0; i--) {
			if(shopCarts[i] != null && shopCarts[i].gameObject != null)
				Destroy(shopCarts[i].gameObject);
		}
		shopCarts.Clear();
		
		var currentRun = DataSaver.s.GetCurrentSave().currentRun;
		
		for (int i = 0; i < fleaMarketLocations.Length; i++) {
			var buildings = fleaMarketLocations[i].GetComponentsInChildren<Cart>();
			for (int j = 0; j < buildings.Length; j++) {
				Destroy(buildings[j].gameObject);
			}
			
			fleaMarketLocations[i].transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
		}


		if (currentRun.isInEndRunArea) {
			for (int i = 0; i < currentRun.shopState.cartStates.Count; i++) { // we don't properly load from carts in cargo delivery thingy
				var cart = currentRun.shopState.cartStates[i];

				var thingy = Instantiate(DataHolder.s.GetCart(cart.state.uniqueName).gameObject, shopableComponentsParent);
				Train.ApplyStateToCart(thingy.GetComponent<Cart>(), cart.state);
				AddCartToShop(thingy.GetComponent<Cart>(), CartLocation.world, false);
				thingy.transform.position = cart.pos;
				thingy.transform.rotation = cart.rot;
				thingy.GetComponent<Rigidbody>().isKinematic = false;
				thingy.GetComponent<Rigidbody>().useGravity = true;
			}
			
		} else {
			for (int i = 0; i < currentRun.shopState.cartStates.Count; i++) {
				var cart = currentRun.shopState.cartStates[i];
				
				var thingy = Instantiate(DataHolder.s.GetCart(cart.state.uniqueName).gameObject, shopableComponentsParent);
				Train.ApplyStateToCart(thingy.GetComponent<Cart>(), cart.state);
				if (cart.location == CartLocation.market) { // we don't properly load from carts in the forge
					var location = GetRandomLocation();
					if (location == null)
						return;

					
					location.GetComponent<SnapCartLocation>().SnapToLocation(thingy);
					AddCartToShop(thingy.GetComponent<Cart>(), CartLocation.market, false);
				} else {
					AddCartToShop(thingy.GetComponent<Cart>(), CartLocation.world, false);
					thingy.transform.position = cart.pos;
					thingy.transform.rotation = cart.rot;
					thingy.GetComponent<Rigidbody>().isKinematic = false;
					thingy.GetComponent<Rigidbody>().useGravity = true;
				}
				
			}


			if (currentRun.shopState.leftCargo != null && currentRun.shopState.leftCargo.cargoReward != null && currentRun.shopState.leftCargo.cargoReward.Length > 0) {
				leftCargo = SpawnCargo(currentRun.shopState.leftCargo, leftCargoParent);
			} else {
				var carts = Train.s.carts;
				for (int i = 0; i < carts.Count; i++) {
					if (carts[i].isCargo) {
						var cargo = carts[i].GetComponentInChildren<CargoModule>();
						if (cargo.isLeftCargo) {
							leftCargo = carts[i];
							ShopStateController.s.SetGoingLeft();
							break;
						}
					}
				}
			}

			if (currentRun.shopState.rightCargo != null && currentRun.shopState.rightCargo.cargoReward != null && currentRun.shopState.rightCargo.cargoReward.Length > 0) {
				rightCargo = SpawnCargo(currentRun.shopState.rightCargo, rightCargoParent);
			} else {
				var carts = Train.s.carts;
				for (int i = 0; i < carts.Count; i++) {
					if (carts[i].isCargo) {
						var cargo = carts[i].GetComponentInChildren<CargoModule>();
						if (!cargo.isLeftCargo) {
							rightCargo = carts[i];
							ShopStateController.s.SetGoingRight();
							break;
						}
					}
				}
			}

			
			SpawnRewardAtPos(leftRewardPos, leftCargo.GetComponentInChildren<CargoModule>());
			SpawnRewardAtPos(rightRewardPos, rightCargo.GetComponentInChildren<CargoModule>());

			CheckIfCanGo();
		}

		SaveShopCartState();
	}

	[ColorUsage(true, true)] 
	public Color rewardOverlayColor = Color.cyan;

	void SpawnRewardAtPos(Transform pos, CargoModule module) {
		pos.DeleteAllChildren();

		var rewardCart = Instantiate(DataHolder.s.GetCart(module.GetReward()), pos);

		rewardCart.canPlayerDrag = false;

		var renderers = rewardCart.GetComponentsInChildren<MeshRenderer>();

		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].material.SetColor("OverlayColor", rewardOverlayColor);
		}

	}

	int CheckIfCanGo() {
		if (DataSaver.s.GetCurrentSave().currentRun.isInEndRunArea) {
			var cargoCount = 0;
			var looseCartCount = shopCarts.Count;
			for (int i = 0; i < shopCarts.Count; i++) {
				if (shopCarts[i].isCargo) {
					cargoCount += 1;
					shopCarts[i].GetComponentInChildren<CargoModule>().HighlightForDelivery();
				}
			}

			for (int i = 0; i < Train.s.carts.Count; i++) {
				if (Train.s.carts[i].isCargo) {
					cargoCount += 1;
					Train.s.carts[i].GetComponentInChildren<CargoModule>().HighlightForDelivery();
				}
			}

			if (looseCartCount > 0 || cargoCount > 0) {
				MissionWinFinisher.s.SetCannotGo(cargoCount > 0);
			} else {
				MissionWinFinisher.s.SetCanGo();
			}
			
			ShopStateController.s.SetCannotGo(ShopStateController.CanStartLevelStatus.needToSelectDestination);

			return cargoCount;

		} else {
			var fleaMarketCount = 0;
			for (int i = 0; i < shopCarts.Count; i++) {
				if (shopCarts[i].myLocation == CartLocation.market) {
					fleaMarketCount += 1;
				}
			}
			
			if (leftCargo.myLocation == CartLocation.train) {
				ShopStateController.s.SetGoingLeft();
			}

			if (rightCargo.myLocation == CartLocation.train) {
				ShopStateController.s.SetGoingRight();
			}

			if (fleaMarketCount < 3) {
				ShopStateController.s.SetCannotGo(ShopStateController.CanStartLevelStatus.needToPutThingInFleaMarket);
			}

			for (int i = 0; i < shopCarts.Count; i++) {
				if (shopCarts[i].myLocation == CartLocation.world) {
					ShopStateController.s.SetCannotGo(ShopStateController.CanStartLevelStatus.needToPickUpFreeCarts);
				}
			}
			
			if (leftCargo.myLocation != CartLocation.train && rightCargo.myLocation != CartLocation.train) {
				ShopStateController.s.SetCannotGo(ShopStateController.CanStartLevelStatus.needToSelectDestination);
			}

			
			MissionWinFinisher.s.SetCannotGo(true);
			return fleaMarketCount;
		}
	}

	Cart SpawnCargo(DataSaver.TrainState.CartState.CargoState cargoState, SnapCartLocation location) {
		var thingy = Instantiate(buildingCargo, shopableComponentsParent);
		thingy.transform.position = location.snapTransform.position;
		thingy.transform.rotation = location.snapTransform.rotation;
		thingy.transform.SetParent(location.snapTransform);
			
		thingy.GetComponentInChildren<CargoModule>().SetCargo(cargoState);
		AddCartToShop(thingy.GetComponent<Cart>(), CartLocation.destinationSelect, false);
		return thingy.GetComponent<Cart>();
	}

	SnapCartLocation GetRandomLocation() {
		var availableCount = 0;
		for (int i = 0; i < fleaMarketLocations.Length; i++) {
			if (fleaMarketLocations[i].IsEmpty())
				availableCount += 1;
		}

		if (availableCount < 0)
			return null;

		var selection = Random.Range(0, availableCount);
		for (int i = 0; i < fleaMarketLocations.Length; i++) {
			if (fleaMarketLocations[i].IsEmpty())
				selection -= 1;
			if (selection <= 0) {
				if (fleaMarketLocations[i].IsEmpty()) {
					return fleaMarketLocations[i];
				}
			}
		}

		return null;
	}

	public string GetRandomPowerup() {
		return DataHolder.s.powerUps[Random.Range(0, DataHolder.s.powerUps.Length)].name;
	}

	class RarityPickChance {
		public float epicChance; 
		public float rareChance;
		public float startingValue;
		public float increaseValue;
	}

	RarityPickChance fleaMarketRarity = new RarityPickChance() {
		epicChance = 0.03f,
		rareChance = 0.37f,
		startingValue = -0.05f,
		increaseValue = 0.01f
	};
	RarityPickChance destinationRarity = new RarityPickChance() {
		epicChance = 0.10f,
		rareChance = 0.40f,
		startingValue = -0.12f,
		increaseValue = 0.02f
	};

	public void SetUpNewCharacterRarityBoosts() {
		DataSaver.s.GetCurrentSave().currentRun.fleaMarketRarityBoost = fleaMarketRarity.startingValue;
		DataSaver.s.GetCurrentSave().currentRun.destinationRarityBoost = destinationRarity.startingValue;
	}

	public string GetRandomBuildingCargoForDestinationReward() {
		return _GetRandomBuildingCargo(tier1Buildings, ref DataSaver.s.GetCurrentSave().currentRun.destinationRarityBoost, destinationRarity);
	}

	public string GetRandomBuildingCargoForFleaMarket() {
		List<string> rollSource = tier1Buildings;

		if (DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar().starChunk == 0) {
			rollSource = firstShopCarts;
		}
		
		return _GetRandomBuildingCargo(rollSource, ref DataSaver.s.GetCurrentSave().currentRun.fleaMarketRarityBoost, fleaMarketRarity);
	}


	private List<string> recentlySpawned = new List<string>();

	string _GetRandomBuildingCargo(List<string> rollSource, ref float rarityBoost, RarityPickChance rarityPickChance) {
		var building = _GetRandomBuildingCargo(rollSource, ref rarityBoost, rarityPickChance, true);

		for (int i = 0; i < 3; i++) {
			if (building.Length > 0) {
				break;
			} else {
				building = _GetRandomBuildingCargo(rollSource, ref rarityBoost, rarityPickChance, true);
			}
		}

		if (building.Length <= 0) {
			building = _GetRandomBuildingCargo(rollSource, ref rarityBoost, rarityPickChance, false);
		}

		if (DataHolder.s.GetCart(building).myRarity == CartRarity.epic) {
			rarityBoost = rarityPickChance.startingValue;
		} else {
			rarityBoost += rarityPickChance.increaseValue;
		}
		
		recentlySpawned.Add(building);
		if (recentlySpawned.Count > 5) {
			recentlySpawned.RemoveAt(0);
		}

		return building;
	}

	string _GetRandomBuildingCargo(List<string> rollSource, ref float rarityBoost, RarityPickChance rarityPickChance, bool careRecentlySpawned) {
		List<string> possibleCarts = new List<string>();
		


		var cartRarityRoll = Random.value;

		if (cartRarityRoll < rarityPickChance.epicChance + rarityBoost) {
			// rolled an epic cart
			for (int i = 0; i < rollSource.Count; i++) {
				if (DataHolder.s.GetCart(rollSource[i]).myRarity == CartRarity.epic) {
					if (!recentlySpawned.Contains(rollSource[i]) || !careRecentlySpawned) {
						possibleCarts.Add(rollSource[i]);
					}
				}
			}
			
		}
		// if there are no carts of the higher tier available, roll a lower tier
		if (possibleCarts.Count == 0 && cartRarityRoll < rarityPickChance.rareChance + (rarityPickChance.epicChance + rarityBoost)) {
			//rolled a rare cart
			
			for (int i = 0; i < rollSource.Count; i++) {
				if (DataHolder.s.GetCart(rollSource[i]).myRarity == CartRarity.rare) {
					if (!recentlySpawned.Contains(rollSource[i]) || !careRecentlySpawned) {
						possibleCarts.Add(rollSource[i]);
					}
				}
			}
			
		}
		// if there are no carts of the higher tier available, roll a lower tier
		if (possibleCarts.Count == 0) {
			//rolled a common cart
			
			for (int i = 0; i < rollSource.Count; i++) {
				if (DataHolder.s.GetCart(rollSource[i]).myRarity == CartRarity.common) {
					if (!recentlySpawned.Contains(rollSource[i]) || !careRecentlySpawned) {
						possibleCarts.Add(rollSource[i]);
					}
				}
			}
		}


		var result = "";

		if (possibleCarts.Count > 0) {
			result = possibleCarts[Random.Range(0, possibleCarts.Count)];
		}
		
		return result;
	}
	
	/*public string[] GetRandomBossRewards() {
		var results = GetBuildingsFromList(bossBuildings);
		return results;
	}*/

	private string[] GetBuildingsFromList(List<string> buildings) {
		var count = 3;
		
		
		var eligibleRewards = new List<string>();
		
		while (eligibleRewards.Count < count) {
			eligibleRewards.AddRange(buildings);
		}
		
		eligibleRewards.Shuffle();
		

		var results = new String[count];
		eligibleRewards.CopyTo(0, results, 0, count);
		return results;
	}
	
	private static IEnumerable GetAllModuleNames() {
		var buildings = GameObject.FindObjectOfType<DataHolder>().buildings;
		var buildingNames = new List<string>();
		buildingNames.Add("");
		for (int i = 0; i < buildings.Length; i++) {
			buildingNames.Add(buildings[i].uniqueName);
		}
		return buildingNames;
	}

	public int WorldCartCount() {
		var count = 0;
		for (int i = 0; i < shopCarts.Count; i++) {
			if (shopCarts[i].myLocation == CartLocation.world)
				count += 1;
		}

		return count;
	}
}
