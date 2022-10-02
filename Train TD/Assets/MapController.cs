using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapController : MonoBehaviour {
	public static MapController s;

	private void Awake() {
		s = this;
	}

	public Transform parent;
	public Transform connectorsParent;

	public GameObject[] starGroups; 
    public GameObject connectingLine;

    public int timePortalDistance = 8;

    public int miniPortalDistance = 3;
    public int miniPortalVariance = 1;

    public int rewardDistance = 2;

    // min stars = timePortalDistance
    public int minStarCount = 12;
    // max stars = timePortalDistance * 3 - (2*(1 + 2))
    public int maxStarCount = 18;


    public LevelDataScriptable[] firstLevels;
    public LevelDataScriptable[] remainingLevels;
    public LevelDataScriptable[] bossLevels;

    public MenuToggle mapUI;

    private void Start() {
	    //GenerateStarMap();
	    ApplyStarMapFromSave();
    }


    [Button]
    public void GenerateStarMap() {
	    var myMap = new List<MapChunk>();
	    
	    GenerateMapWithWaveCollapse(myMap);
	    
	    var starMapState = GenerateStarMapStateFromMap(myMap);
	    ConnectAllStars(starMapState);
	    
	    PutPlayerInStar(starMapState.chunks[0].myStars[0]);
	    
	    var shops = MakeShops(starMapState);
	    MakeBoss(starMapState);

	    DataSaver.s.GetCurrentSave().currentRun.map = starMapState;
	    Debug.Log($"Star map instantiation complete");

	    ApplyStarMapFromSave();
    }

    public void ApplyStarMapFromSave() {
	    SpawnStarsAndConnections(DataSaver.s.GetCurrentSave().currentRun.map);
	    //targetStarInfoScreen.Hide();
    }

    private StarMapState GenerateStarMapStateFromMap(List<MapChunk> myMap) {
	    var starMapState = new StarMapState();
	    for (int i = 0; i < myMap.Count; i++) {
		    var chunk = new StarMapChunk();

		    for (int j = 0; j < myMap[i].possibleStarCounts[0]; j++) {
			    chunk.myStars.Add(CreateStarState(i));
		    }

		    starMapState.chunks.Add(chunk);
	    }

	    return starMapState;
    }

    void ConnectAllStars(StarMapState starMapState) {
	    for (int i = 0; i < starMapState.chunks.Count - 1; i++) {
		    ConnectStarGroups(starMapState.chunks[i], starMapState.chunks[i + 1]);
	    }
    }

    private void MakeBoss(StarMapState starMapState) {
	    MakeBossStar(starMapState.chunks[timePortalDistance - 1].myStars[0]);
    }

    void MakeBossStar(StarState starState) {
	    starState.isBoss = true;
	    starState.level = bossLevels[Random.Range(0, bossLevels.Length)].myData;
	    //starState.enemyShip = sectorBoss;
    }

    readonly string[] prefixes = new []{"Slam", "Slo","Ose","Yellow","Flu", "Nice", "Bad", "Great", "Bear"};
	readonly string[] suffixes = new []{"ford", "hill", "ville", "wood", "burg", "bridge", "ton"};
	private HashSet<string> givenStarNames = new HashSet<string>();

    private StarState CreateStarState(int starChunk) {

	    LevelData level;
	    if (starChunk < 2) {
		    level = firstLevels[Random.Range(0, firstLevels.Length)].myData;
	    } else {
		    level = remainingLevels[Random.Range(0, remainingLevels.Length)].myData;
	    }

	    


	    var count = 0;
	    var potentialStarName = $"{prefixes[Random.Range(0, prefixes.Length)]}{suffixes[Random.Range(0, suffixes.Length)]}";
	    while (givenStarNames.Contains(potentialStarName) && count < 100) {
		    count += 1;
		    potentialStarName = $"{prefixes[Random.Range(0, prefixes.Length)]}{suffixes[Random.Range(0, suffixes.Length)]}";
	    }

	    if (count >= 100) {
		    potentialStarName = "Kekville";
	    }

	    givenStarNames.Add(potentialStarName);


	    var info = new StarState(potentialStarName);
	    info.level = level;
	    
	    if (starChunk % 2 == 1) {
		    info.rewardCart = 1;
	    }
	    
	    return info;
    }

    private List<int> MakeShops(StarMapState starMapState) {
	    var miniPortalLocations = new List<int>();

	    for (int i = miniPortalDistance; i < timePortalDistance; i += miniPortalDistance) {
		    miniPortalLocations.Add(i + Random.Range(-miniPortalVariance, +miniPortalVariance));
	    }

	    for (int i = 0; i < miniPortalLocations.Count; i++) {
		    var shopCandidates = starMapState.chunks[miniPortalLocations[i]].myStars;
		    MakeShopStar(shopCandidates[Random.Range(0, shopCandidates.Count)]);
	    }

	    return miniPortalLocations;
    }
    

    void MakeShopStar(StarState starState) {
	    starState.isShop = true;
    }

    private void SpawnStarsAndConnections(StarMapState starMapState) {
	    parent.DeleteAllChildren();
	    connectorsParent.DeleteAllChildren();

	    var activeStarGroups = new List<GameObject>();

	    for (int i = 0; i < starMapState.chunks.Count; i++) {
		    var chunk = starMapState.chunks[i];
		    activeStarGroups.Add(Instantiate(starGroups[chunk.myStars.Count - 1], parent));

		    var stars = activeStarGroups[i].GetComponentsInChildren<MiniGUI_Star>();
		    for (int j = 0; j < stars.Length; j++) {
			    var myStar = chunk.myStars[j];
			    stars[j].Initialize(myStar);
		    }
	    }

	    for (int i = 0; i < activeStarGroups.Count-1; i++) {
		    var stars = activeStarGroups[i].GetComponentsInChildren<MiniGUI_Star>();
		    for (int j = 0; j < stars.Length; j++) {
			    var curStar = stars[j];

			    var nextStars = activeStarGroups[i + 1].GetComponentsInChildren<MiniGUI_Star>();

			    for (int k = 0; k < curStar.myInfo.outgoingConnections.Count; k++) {
				    var a = curStar;
				    var b = GetStarFromStarName(nextStars, curStar.myInfo.outgoingConnections[k]);
				    
				    var connection = Instantiate(connectingLine, connectorsParent).GetComponent<MiniGUI_StarConnection>();
				    connection.Initialize(a,b);

				    if (a.myInfo.isPlayerHere) {
					    connection.SetHighlightState(true);
					    b.currentlyPlayerTravelable = true;
				    }else if (a.myInfo.previouslyVisited && b.myInfo.previouslyVisited) {
					    connection.SetPreviouslyVisited();
				    }
			    }
		    }
	    }
    }

    MiniGUI_Star GetStarFromStarName(MiniGUI_Star[] stars, string starName) {
	    for (int i = 0; i < stars.Length; i++) {
		    if (stars[i].myInfo.starName == starName) {
			    return stars[i];
		    }
	    }

	    return null;
    }

    private void GenerateMapWithWaveCollapse(List<MapChunk> myMap) {
	    var totalStarCount = 0;
	    var curGeneration = 0;
	    while ((totalStarCount < minStarCount || totalStarCount > maxStarCount) && curGeneration < 20) {
		    myMap.Clear();
		    for (int i = 0; i < timePortalDistance; i++) {
			    myMap.Add(new MapChunk());
		    }

		    myMap[timePortalDistance - 1].possibleStarCounts = new List<int>() { 1 };
		    myMap[0].possibleStarCounts = new List<int>() { 1 };

		    WaveFunctionCollapse(myMap, timePortalDistance - 1, 1);
		    WaveFunctionCollapse(myMap, 0, 1);

		    var randomPoint = Random.Range(1, timePortalDistance);
		    var randomStarCount = Random.Range(1, 4);
		    while (!WaveFunctionCollapse(myMap, randomPoint, randomStarCount)) {
			    randomPoint = Random.Range(1, timePortalDistance);
			    randomStarCount = Random.Range(1, 4);
		    }

		    totalStarCount = 0;
		    for (int i = 0; i < myMap.Count; i++) {
			    totalStarCount += myMap[i].possibleStarCounts[0];
		    }

		    curGeneration += 1;
	    }

	    Debug.Log($"Generated Star Map with {totalStarCount} stars in {curGeneration} generations");
    }


    public MiniGUI_StarInfoPanel targetStarInfoScreen;

    //public StarState playerStar => StateMaster.s.currentState.starMapState.GetPlayerStar();
    public StarState playerStar => DataSaver.s.GetCurrentSave().currentRun.map.GetPlayerStar();
	private StarState targetStar;
    public void ShowStarInfo(StarState star) {
	    targetStar = star;
	    targetStarInfoScreen.Initialize(targetStar);
    }

    public void TravelToStar() {
	    RemovePlayerFromStar();
	    PutPlayerInStar(targetStar);
	    
	    targetStarInfoScreen.Hide();
	    mapUI.HideMenu();
	    
	    ApplyStarMapFromSave();
	    StarterUIController.s.SelectLevelAndStart(targetStar.level);
    }

    public void RemovePlayerFromStar() {
	    playerStar.isPlayerHere = false;
    }
    
    public void PutPlayerInStar(StarState starState) {
	    starState.isPlayerHere = true;
	    starState.previouslyVisited = true;
    }


    bool WaveFunctionCollapse(List<MapChunk> map, int index, int starCount) {
	    if (map[index].possibleStarCounts.Contains(starCount)) {
		    map[index].possibleStarCounts = new List<int>() { starCount};
	    } else {
		    return false;
	    }

	    
	    for (int i = index+1; i < map.Count; i++) {
		    var a = map[i-1];
		    var b = map[i];
		    
		    CollapseTwoChunks(a, b);
	    }


	    for (int i = index - 1; i >= 0; i--) {
		    var a = map[i+1];
		    var b = map[i];
		    
		    CollapseTwoChunks(a, b);
	    }


	    var fullyCollapsed = true;
	    for (int i = 0; i < map.Count; i++) {
		    if (map[i].possibleStarCounts.Count > 1) {
			    fullyCollapsed = false;
			    break;
		    }
	    }

	    return fullyCollapsed;
    }

    private static void CollapseTwoChunks(MapChunk a, MapChunk b) {
	    var legalSpots = new List<int>();
	    for (int j = 0; j < a.possibleStarCounts.Count; j++) {
		    var sourceNum = a.possibleStarCounts[j];
		    for (int k = 0; k < b.possibleStarCounts.Count; k++) {
			    var compNum = b.possibleStarCounts[k];
			    if (MapChunk.isLegalNeighbor(sourceNum, compNum)) {
				    if(!legalSpots.Contains(compNum))
						legalSpots.Add(compNum);
			    }
		    }
	    }
	    b.possibleStarCounts = legalSpots;
    }

    void ConnectStarGroups(StarMapChunk a, StarMapChunk b) {
	    var aStars = a.myStars;
	    var bStars = b.myStars;

	    var randomLayout = Random.Range(0, 100);
	    switch (aStars.Count) {
		    case 1:
			    switch (bStars.Count) {
				    case 1: // a = 1, b = 1
					    ConnectStars(aStars[0], bStars[0]);
					    break;
				    case 2: // a = 1, b = 2
					    ConnectStars(aStars[0], bStars[0]);
					    ConnectStars(aStars[0], bStars[1]);
					    break;
			    }
			    break;
		    case 2:
			    switch (bStars.Count) {
				    case 1: // a = 2, b = 1
					    ConnectStars(aStars[0], bStars[0]);
					    ConnectStars(aStars[1], bStars[0]);
					    break;
				    case 2: // a = 2, b = 2
					    switch (randomLayout % 3) {
						    case 0:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    break;
						    case 1:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[0], bStars[1]);
							    ConnectStars(aStars[1], bStars[1]);
							    break;
						    case 2:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    break;
					    }
					    break;
				    case 3: // a = 2, b = 3
					    switch (randomLayout % 3) {
						    case 0:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[0], bStars[1]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[1], bStars[2]);
							    break;
						    case 1:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[1], bStars[2]);
							    break;
						    case 2:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[0], bStars[1]);
							    ConnectStars(aStars[1], bStars[2]);
							    break;
					    }
					    break;
			    }
			    break;
		    case 3:
			    switch (bStars.Count) {
				    case 2: // a = 3, b = 2
					    switch (randomLayout % 3) {
						    case 0:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[2], bStars[1]);
							    break;
						    case 1:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[2], bStars[1]);
							    break;
						    case 2:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[0]);
							    ConnectStars(aStars[2], bStars[1]);
							    break;
					    }
					    break;
				    case 3: // a = 3, b = 3
					    switch (randomLayout % 5) {
								// all possibilities
							    /*ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[0], bStars[1]);
							    ConnectStars(aStars[1], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[1], bStars[2]);
							    ConnectStars(aStars[2], bStars[1]);
							    ConnectStars(aStars[2], bStars[2]);*/
						    case 0:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[2], bStars[2]);
							    break;
						    case 1:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[1], bStars[2]);
							    ConnectStars(aStars[2], bStars[2]);
							    break;
						    case 2:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[0], bStars[1]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[1], bStars[2]);
							    ConnectStars(aStars[2], bStars[2]);
							    break;
						    case 3:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[1], bStars[0]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[2], bStars[1]);
							    ConnectStars(aStars[2], bStars[2]);
							    break;
						    case 4:
							    ConnectStars(aStars[0], bStars[0]);
							    ConnectStars(aStars[0], bStars[1]);
							    ConnectStars(aStars[1], bStars[1]);
							    ConnectStars(aStars[2], bStars[1]);
							    ConnectStars(aStars[2], bStars[2]);
							    break;
					    }
					    break;
			    }
			    break;
	    }
    }

    private void ConnectStars(StarState a, StarState b) {
	    a.outgoingConnections.Add(b.starName);
    }

    class MapChunk {
	    public List<int> possibleStarCounts = new List<int>(){3,2,1};

	    public static bool isLegalNeighbor(int a, int b) {
		    return Mathf.Abs(a - b) <= 1;
	    }
    }
}



[Serializable]
public class StarMapState {
	public List<StarMapChunk> chunks = new List<StarMapChunk>();
	/*public StarState GetStarStateFromStarName(string starName) {
		for (int i = 0; i < chunks.Count; i++) {
			for (int j = 0; j < chunks[i].myStars.Count; j++) {
				if (chunks[i].myStars[j].starName == starName)
					return chunks[i].myStars[j];
			}
		}
		return null;
	}*/

	public StarState GetPlayerStar() {
		for (int i = 0; i < chunks.Count; i++) {
			for (int j = 0; j < chunks[i].myStars.Count; j++) {
				if (chunks[i].myStars[j].isPlayerHere)
					return chunks[i].myStars[j];
			}
		}
		return null;
	}
}


[Serializable]
public class StarMapChunk {
	public List<StarState> myStars = new List<StarState>();
}

[Serializable]
public class StarState {
	[Tooltip("Must be Unique")]
	public string starName;
	public bool isShop;
	public bool isBoss;
	public bool isPlayerHere;
	public bool previouslyVisited;
	public int starVariation = -1;
	public LevelData level;
	public int rewardCart = 0;

	public List<string> outgoingConnections = new List<string>();

	public StarState(string _starName) {
		starName = _starName;
	}
}


