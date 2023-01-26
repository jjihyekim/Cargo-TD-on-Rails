using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
        profileMenu, starterMenu, levelInProgress, levelFinished
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
    
    public bool isStarterMenu() {
        return myGameState == GameState.starterMenu;
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
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Init()
    {
        Debug.Log("Constant Objects Singleton Reset");
        s = null;
    }
    
    private void Awake() {
        ClearLevel();
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
    
    public void SetCurrentLevel(LevelData levelData) {
        _currentLevel = levelData.Copy();

        if (MiniGUI_DebugLevelName.s != null) {
            MiniGUI_DebugLevelName.s.SetLevelName(_currentLevel.levelName);
        }
    }

    void ClearLevel() {
        _currentLevel = null;
    }

    public GameObject loadingScreen;
    public GameObject loadingText;
    public CanvasGroup canvasGroup;


    public void OpenProfileScreen(bool hardLoad = false) {
        StopAllCoroutines();
        _gameState = GameState.profileMenu;
        if (hardLoad) {
            LoadScene(mainScene);
        } else {
            StartCoroutine(OnlyFade(false, () => DoOpenProfileMenu()));
        }
    }

    void DoOpenProfileMenu() {
        ProfileSelectionMenu.s.OpenProfileMenu();
    }

    public void BackToStarterMenu(bool showLoading = false) {
        _gameState = GameState.starterMenu;
        
        StopAllCoroutines();
        afterTransferCalls.Clear();
        StartCoroutine(OnlyFade(showLoading, () => DoTransfer()));
        //LoadScene(mainScene, true);
    }


    public Queue<Action> afterTransferCalls = new Queue<Action>(); // must add stuff to this AFTER calling back to menu!
    void DoTransfer() {
        ClearLevel();
        
        while (afterTransferCalls.TryDequeue(out Action result)) {
            result(); 
        }
        
        WorldMapCreator.s.ReturnToRegularMap();
        StarterUIController.s.OpenStarterUI();
    }

    public float fadeTime = 0.2f;
    IEnumerator OnlyFade(bool showLoading, Action toCallInTheMiddle) {
        isLoading = true;
        loadingProgress = 0;
        if(!showLoading)
            loadingText.SetActive(false);
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(currentFadeValue,1, fadeTime-0.01f));

        yield return null; // one frame pause
        
        toCallInTheMiddle();

        yield return StartCoroutine(FadeLoadingScreen(1,0, fadeTime));
        loadingScreen.SetActive(false);
        loadingText.SetActive(true);
        isLoading = false;
    }

    public void ForceReloadScene() {
        LoadScene(mainScene,true);
    }

    public static float loadingProgress;
    public bool isLoading = false;
    private void LoadScene(SceneReference sceneReference, bool isForced = false) {
        if (!isLoading || isForced) {
            StopAllCoroutines();
            StartCoroutine(StartLoad(sceneReference));
        }
    }

    public float currentFadeValue = 0f;
    IEnumerator StartLoad(SceneReference sceneReference) {
        isLoading = true;
        loadingProgress = 0;
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(currentFadeValue,1, 0.5f));

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
            currentFadeValue = canvasGroup.alpha;
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetValue;
    }
}
