using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayStateMaster : MonoBehaviour {
    public static PlayStateMaster s;

    private void Awake() {
        s = this;
    }
    

    public UnityEvent OnMainMenuEntered = new UnityEvent();
    public UnityEvent OnCharacterSelected = new UnityEvent();
    public UnityEvent OnDrawWorld = new UnityEvent();
    public UnityEvent OnNewWorldCreation = new UnityEvent();
    public UnityEvent OnShopEntered = new UnityEvent();
    public UnityEvent OnCombatEntered = new UnityEvent();
    
    public UnityEvent OnCombatWon = new UnityEvent();
    public UnityEvent OnCombatLost = new UnityEvent();
    
    public UnityEvent OnCombatFinished = new UnityEvent();
    public UnityEvent OnCombatCleanup = new UnityEvent();
    
    
    [SerializeField]
    private ConstructedLevel _currentLevel;

    public ConstructedLevel currentLevel {
        get {
            return _currentLevel;
        }
    }

    public enum GameState {
        mainMenu, shop, combat, levelFinished
    }

    [SerializeField] private GameState _gameState;

    
    public GameState myGameState {
        get {
            return _gameState;
        }
    }

    public bool isCombatStarted() {
        return myGameState == GameState.combat || myGameState == GameState.levelFinished;
    }
    public bool isCombatFinished() {
        return myGameState == GameState.levelFinished;
    }
    
    public bool isShop() {
        return myGameState == GameState.shop;
    }
    
    public bool isMainMenu() {
        return myGameState == GameState.mainMenu;
    }

    public void StarCombat() {
        OnCombatEntered?.Invoke();
        _gameState = GameState.combat;
    }

    public void FinishCombat() {
        OnCombatFinished?.Invoke();
        _gameState = GameState.levelFinished;
    }

    public bool isCombatInProgress() {
        return myGameState == GameState.combat;
    }
    
    public bool IsLevelSelected() {
        return _currentLevel != null;
    }

    public void SetCurrentLevel(ConstructedLevel levelData) {
        _currentLevel = levelData.Copy();

        if (MiniGUI_DebugLevelName.s != null) {
            MiniGUI_DebugLevelName.s.SetLevelName(_currentLevel.levelName);
        }
    }

    public void ClearLevel() {
        _currentLevel = null;
    }
    
    public void OpenMainMenu() {
        StopAllCoroutines();
        
        StartCoroutine(Transition(false, () => DoOpenMainMenu()));
    }

    void DoOpenMainMenu() {
        MainMenu.s.OpenProfileMenu();
        _gameState = GameState.mainMenu;
        OnMainMenuEntered?.Invoke();
    }


    public void ClearOutOfCombat() {
        _gameState = GameState.shop;
        
        StopAllCoroutines();
        StartCoroutine(Transition(false, () => {
            OnCombatCleanup?.Invoke();
            OnShopEntered?.Invoke();
        }));
    }

    public void EnterShopState() {
        _gameState = GameState.shop;

        StopAllCoroutines();

        if (DataSaver.s.GetCurrentSave().isInARun) {
            if (WorldGenerationProgress() >= 1f) {
                StartCoroutine(Transition(false, () => OnShopEntered?.Invoke()));
            } else {
                
                StartCoroutine(Transition(true, () => {
                    OnDrawWorld?.Invoke();
                    OnShopEntered?.Invoke();
                }, WorldGenerationProgress));
            }
        } else {
            StartCoroutine(Transition(false, () => {
                CharacterSelector.s.CheckAndShowCharSelectionScreen();
                MainMenu.s.ExitMainMenu();
            }));
        }
    }


    public void FinishCharacterSelection() {
        _gameState = GameState.shop;
        
        StopAllCoroutines();
        WorldMapCreator.s.worldMapGenerationProgress = 0;
        StartCoroutine(Transition(true, () => {
            OnNewWorldCreation?.Invoke();
            OnShopEntered?.Invoke();
            CharacterSelector.s.CheckAndShowCharSelectionScreen();
        }, WorldGenerationProgress));
    }

    public void EnterNewAct() {
        _gameState = GameState.shop;
        
        StopAllCoroutines();
        WorldMapCreator.s.worldMapGenerationProgress = 0;
        StartCoroutine(Transition(true, () => {
            OnNewWorldCreation?.Invoke();
            OnShopEntered?.Invoke();
        }, WorldGenerationProgress));
    }

    float WorldGenerationProgress() {
        return WorldMapCreator.s.worldMapGenerationProgress;
    }

    delegate float LoadDelegate();
    

    public Queue<Action> afterTransferCalls = new Queue<Action>(); // must add stuff to this AFTER calling back to menu!
    void DoTransfer() {
        while (afterTransferCalls.TryDequeue(out Action result)) {
            result(); 
        }
    }

    public bool isLoading = false;
    public float loadingProgress;
    public Slider loadingSlider;
    public GameObject loadingScreen;
    public GameObject loadingText;
    public CanvasGroup canvasGroup;
    public float currentFadeValue;
    public float fadeTime = 0.2f;
    IEnumerator Transition(bool showLoading, Action toCallInTheMiddle, LoadDelegate loadProgress = null) {
        isLoading = true;
        loadingProgress = 0;
        loadingSlider.value = loadingProgress;
        if(!showLoading)
            loadingText.SetActive(false);
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(currentFadeValue,1, fadeTime-0.01f));

        yield return null; // one frame pause
        
        toCallInTheMiddle();

        if (showLoading && loadProgress != null) {
            while (loadingProgress < 1f) {
                loadingProgress = loadProgress();
                loadingSlider.value = loadingProgress;
                yield return null;
            }
        }

        yield return StartCoroutine(FadeLoadingScreen(1,0, fadeTime));
        loadingScreen.SetActive(false);
        loadingText.SetActive(true);
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
