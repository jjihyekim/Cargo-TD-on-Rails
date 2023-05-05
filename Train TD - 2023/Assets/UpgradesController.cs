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
	

	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier1Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier2Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> tier3Buildings = new List<string>();
	[ValueDropdown("GetAllModuleNames")]
	public List<string> bossBuildings = new List<string>();

	public Transform cargoLocationsParent;
	private FleaMarketSlot[] _cargoLocations;
	public Transform shopableComponentsParent;
	public GameObject buildingCargo;

	public SnapCartLocation leftCargoParent;
	public SnapCartLocation rightCargoParent;

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

	public void UpdateCartShopHighlights() {
		for (int i = 0; i < _cargoLocations.Length; i++) {
			var isLocationEmpty = _cargoLocations[i].GetComponent<SnapCartLocation>().snapTransform.childCount == 0;
			_cargoLocations[i].SetEmptyStatus(isLocationEmpty);
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
			isBuildingCargo = cargo.IsBuildingReward(),
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
			if (cart.isCargo) {
				var cargo = cart.GetComponentInChildren<CargoModule>();
				if (cargo.isLeftCargo) { 
					shopState.leftCargo = new DataSaver.TrainState.CartState.CargoState() {
						isBuildingCargo =  cargo.IsBuildingReward(),
						cargoReward = cargo.GetReward(),
						isLeftCargo = cargo.isLeftCargo
					};
				} else { 
					shopState.rightCargo = new DataSaver.TrainState.CartState.CargoState() {
						isBuildingCargo =  cargo.IsBuildingReward(),
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


	private void Start() {
		_cargoLocations = cargoLocationsParent.GetComponentsInChildren<FleaMarketSlot>();
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
		train = 0, market = 1, world = 2, forge = 3, destinationSelect = 4
	}
	
	void InitializeShop(DataSaver.RunState state) {
		state.shopState = new ShopState();

		var buildingCargoCount = 3; 

		for (int i = 0; i < buildingCargoCount; i++) {
			state.shopState.cartStates.Add(new WorldCartState() {
				location =  CartLocation.market,
				state = new DataSaver.TrainState.CartState() {
					uniqueName = GetRandomBuildingCargo()
				}
			});
		}

		state.shopState.leftCargo = new DataSaver.TrainState.CartState.CargoState() {
			isBuildingCargo =  true,
			cargoReward = GetRandomBuildingCargo(),
			isLeftCargo = true
		};
		state.shopState.rightCargo = new DataSaver.TrainState.CartState.CargoState() {
			isBuildingCargo =  true,
			cargoReward = GetRandomBuildingCargo(),
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

	void SpawnShopItems() {
		transform.DeleteAllChildren();
		for (int i = shopCarts.Count-1; i >= 0; i--) {
			if(shopCarts[i] != null && shopCarts[i].gameObject != null)
				Destroy(shopCarts[i].gameObject);
		}
		shopCarts.Clear();
		
		var currentRun = DataSaver.s.GetCurrentSave().currentRun;
		
		for (int i = 0; i < _cargoLocations.Length; i++) {
			var buildings = _cargoLocations[i].GetComponentsInChildren<Cart>();
			for (int j = 0; j < buildings.Length; j++) {
				Destroy(buildings[j].gameObject);
			}
			
			_cargoLocations[i].transform.rotation = Quaternion.Euler(0,Random.Range(0,360),0);
		}
		

		for (int i = 0; i < _cargoLocations.Length; i++) {
			_cargoLocations[i].isOccupied = false;
		}
		
		
		for (int i = 0; i < currentRun.shopState.cartStates.Count; i++) {
			var cart = currentRun.shopState.cartStates[i];
			var location = GetRandomLocation();
			if(location == null)
				return;

			var thingy = Instantiate(DataHolder.s.GetCart(cart.state.uniqueName).gameObject, shopableComponentsParent);
			Train.ApplyStateToCart(thingy.GetComponent<Cart>(), cart.state);
			location.GetComponent<SnapCartLocation>().SnapToLocation(thingy);
			AddCartToShop(thingy.GetComponent<Cart>(), CartLocation.market, false);
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

		CheckIfCanGo();
		
		SaveShopCartState();
	}


	void CheckIfCanGo() {
		bool canGo = true;
		var fleaMarketCount = 0;
		for (int i = 0; i < shopCarts.Count; i++) {
			if (shopCarts[i].myLocation == CartLocation.market) {
				fleaMarketCount += 1;
			}
		}

		if (fleaMarketCount < 3) {
			ShopStateController.s.SetCannotGo(ShopStateController.CanStartLevelStatus.needToPutThingInFleaMarket);
			canGo = false;
		}
		
		for (int i = 0; i < shopCarts.Count; i++) {
			if (shopCarts[i].myLocation == CartLocation.world) {
				ShopStateController.s.SetCannotGo(ShopStateController.CanStartLevelStatus.needToPickUpFreeCarts);
				canGo = false;
			}
		}
		
		if (leftCargo.myLocation != CartLocation.train && rightCargo.myLocation != CartLocation.train) {
			ShopStateController.s.SetCannotGo(ShopStateController.CanStartLevelStatus.needToSelectDestination);
			canGo = false;
		}

		if (canGo) {
			// dont do anything this is not where this is set
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

	FleaMarketSlot GetRandomLocation() {
		var availableCount = 0;
		for (int i = 0; i < _cargoLocations.Length; i++) {
			if (!_cargoLocations[i].isOccupied)
				availableCount += 1;
		}

		if (availableCount < 0)
			return null;

		var selection = Random.Range(0, availableCount);
		for (int i = 0; i < _cargoLocations.Length; i++) {
			if (!_cargoLocations[i].isOccupied)
				selection -= 1;
			if (selection <= 0) {
				if (!_cargoLocations[i].isOccupied) {
					_cargoLocations[i].isOccupied = true;
					return _cargoLocations[i];
				}
			}
		}

		return null;
	}
	
	public void AddModulesToAvailableModules(string buildingUniqueName, int count) {
		var curRun = DataSaver.s.GetCurrentSave().currentRun;
		TrainModuleHolder myHolder = null;
		for (int i = 0; i < curRun.trainBuildings.Count; i++) {
			if (curRun.trainBuildings[i].moduleUniqueName == buildingUniqueName) {
				myHolder = curRun.trainBuildings[i];
				break;
			}
		}

		if (myHolder == null) {
			myHolder = new TrainModuleHolder();
			myHolder.moduleUniqueName = buildingUniqueName;
			myHolder.amount = 0;
			curRun.trainBuildings.Add(myHolder);
		}

		myHolder.amount += 1;
		
		DataSaver.s.SaveActiveGame();
	}

	public void GetBuilding(string buildingUniqueName) {
		AddModulesToAvailableModules(buildingUniqueName, 1);
	}

	public string GetRandomPowerup() {
		return DataHolder.s.powerUps[Random.Range(0, DataHolder.s.powerUps.Length)].name;
	}
	
	public string GetRandomBuildingCargo() {
		string results;

		switch (DataSaver.s.GetCurrentSave().currentRun.currentAct) {
			case 1:
				results = tier1Buildings[Random.Range(0,tier1Buildings.Count)];
				break;
			case 2:
				if (Random.value > 0.5f) {
					results = tier2Buildings[Random.Range(0,tier2Buildings.Count)];
				} else {
					results = tier1Buildings[Random.Range(0,tier1Buildings.Count)];
				}
				break;
			case 3:
				if (Random.value > 0.5f) {
					results = tier3Buildings[Random.Range(0,tier3Buildings.Count)];
				} else if(Random.value > 0.5f){
					results = tier2Buildings[Random.Range(0,tier2Buildings.Count)];
				} else {
					results = tier1Buildings[Random.Range(0,tier1Buildings.Count)];
				}
				//results = tier3Buildings[Random.Range(0,tier3Buildings.Count)];
				break;
			default:
				results = tier3Buildings[Random.Range(0,tier3Buildings.Count)];
				Debug.LogError($"Illegal Act Number {DataSaver.s.GetCurrentSave().currentRun.currentAct}");
				break;
		}
		
		return results;
	}
	
	public string[] GetRandomLevelRewards() {
		string[] results;

		switch (DataSaver.s.GetCurrentSave().currentRun.currentAct) {
			case 1:
				results = GetBuildingsFromList(tier1Buildings);
				break;
			case 2:
				if (Random.value > 0.5f) {
					results = GetBuildingsFromList(tier2Buildings);
				} else {
					results = GetBuildingsFromList(tier1Buildings);
				}
				break;
			case 3:
				if (Random.value > 0.5f) {
					results = GetBuildingsFromList(tier3Buildings);
				} else if(Random.value > 0.5f){
					results = GetBuildingsFromList(tier2Buildings);
				} else {
					results = GetBuildingsFromList(tier1Buildings);
				}
				results = GetBuildingsFromList(tier3Buildings);
				break;
			default:
				results = GetBuildingsFromList(tier3Buildings);
				Debug.LogError($"Illegal Act Number {DataSaver.s.GetCurrentSave().currentRun.currentAct}");
				break;
		}
		
		return results;
	}

	public string[] GetRandomBossRewards() {
		var results = GetBuildingsFromList(bossBuildings);
		return results;
	}

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
}
