using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {
    public static SceneLoader s;
    
    public SceneReference mainScene;
    
    
    [SerializeField]
    private SceneReference initialScene;

    public bool isLevelInProgress {
        get {
            return myGameState == GameState.levelInProgress;
        }
    }
    
    [SerializeField]
    private LevelData _currentLevel;

    public LevelData currentLevel {
        get {
            return _currentLevel;
        }
    }

    public enum GameState {
        profileMenu, mainMenu, levelInProgress, levelFinished
    }

    [SerializeField] private GameState _gameState;

    public bool autoOpenProfiles;
    
    public GameState myGameState {
        get {
            return _gameState;
        }
    }

    public bool isLevelStarted() {
        return myGameState == GameState.levelInProgress || myGameState == GameState.levelFinished;
    }
    public bool isLevelFinished() {
        return myGameState == GameState.levelFinished;
    }
    
    public bool isProfileMenu() {
        return myGameState == GameState.profileMenu;
    }

    public void StartLevel() {
        _gameState = GameState.levelInProgress;
    }

    public void FinishLevel() {
        _gameState = GameState.levelFinished;
    }
    
    private void Awake() {
        if (s == null) {
            s = this;
            DontDestroyOnLoad(gameObject);

            if (SceneManager.GetActiveScene().path == initialScene.ScenePath) {
                LoadScene(mainScene);
            } else {
                loadingScreen.SetActive(false);
            }
        } else if(s!= this){
            loadingScreen.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void Start() {
        DataSaver.loadEvent += GetCurrentLevelFromSave;
        GetCurrentLevelFromSave();
    }

    void GetCurrentLevelFromSave() {
        var lastLoadedLevelName = DataSaver.s.GetCurrentSave().lastLoadedLevelName;

        for (int i = 0; i < LevelDataLoader.s.allLevels.Count; i++) {
            var thisLevel = LevelDataLoader.s.allLevels[i];
            if (thisLevel.levelName == lastLoadedLevelName) {
                _currentLevel = thisLevel;
                return;
            }
        }

        _currentLevel = LevelDataLoader.s.allLevels[0];
    }

    public void SetCurrentLevel(LevelData levelData) {
        _currentLevel = levelData;
        DataSaver.s.GetCurrentSave().lastLoadedLevelName = _currentLevel.levelName;
    }


    public GameObject loadingScreen;
    public CanvasGroup canvasGroup;


    public void OpenProfileScreen() {
        _gameState = GameState.profileMenu;
        LoadScene(mainScene);
    }
    
    public void BackToMenu() {
        _gameState = GameState.mainMenu;
        LoadScene(mainScene);
    }

    public static float loadingProgress;
    public bool isLoading = false;
    private void LoadScene(SceneReference sceneReference) {
        if(!isLoading)
            StartCoroutine(StartLoad(sceneReference));
    }

    IEnumerator StartLoad(SceneReference sceneReference) {
        isLoading = true;
        loadingProgress = 0;
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(0,1, 0.5f));

        var operation = SceneManager.LoadSceneAsync(sceneReference.ScenePath, LoadSceneMode.Single);
        while (!operation.isDone)
        {
            loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);
            yield return null;
        }

        yield return StartCoroutine(FadeLoadingScreen(1,0, 0.5f));
        loadingScreen.SetActive(false);
        isLoading = false;
    }

    IEnumerator FadeLoadingScreen(float startValue, float targetValue, float duration)
    {
        float time = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetValue;
    }
}
